using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using ModalStrike.Protobuf2;
using ModalStrikeServer.RpcServer.Events;
using ModalStrikeServer.RpcServer.Events.Core;
using ModalStrikeServer.RpcServer.Exceptions.Core;
using ModalStrikeServer.RpcServer.Services;
using ModalStrikeServer.RpcServer.Utilities;
using ModalStrikeServer.RpcServer.Utilities.CustomLogger;
using Exception = ModalStrike.Protobuf2.Exception;

namespace ModalStrikeServer.RpcServer.Core {
    public class TCPServer {
        private bool _pongSignal = true;

        private static readonly object StateLock = new object();

        private readonly Stopwatch _pingPongWatch = new();

        private readonly Stopwatch _sessionTimer = new();

        private bool _clientConnected = false;

        private readonly Dictionary<string, RpcService> _rpcHandlers = new();

        private readonly TcpClient _tcpClient;

        private readonly ConcurrentQueue<RpcRequest> _requests = new();

        private readonly ConcurrentQueue<ResponseMessage> _responses = new();
        
        private string _playerId = string.Empty;
        
        public TcpClient TcpClient => _tcpClient;

        public int Timeout { get; set; } = 11000;

        public long RequestTimeout { get; set; } = 10000L;


        public TCPServer(TcpClient client) {
            _tcpClient = client ?? throw new ArgumentNullException(nameof(client));
            _clientConnected = true;
            InitializeRpcHandlers();
        }

        private void InitializeRpcHandlers() {
            _rpcHandlers.Add("HandshakeRemoteService", new HandshakeRemoteService(this, new ServiceLogger(typeof(HandshakeRemoteService))));
            _rpcHandlers.Add("AuthRemoteService", new AuthRemoteSerivce(this, new ServiceLogger(typeof(AuthRemoteSerivce))));
            _rpcHandlers.Add("PlayerRemoteService", new PlayerRemoteService(this, new ServiceLogger(typeof(PlayerRemoteService))));
        }

        private bool PingPongCheck() {
            if(_pongSignal) {
                if(_pingPongWatch.IsRunning) {
                    _pingPongWatch.Reset();
                }
                return true;
            }

            if(!_pingPongWatch.IsRunning) {
                _pingPongWatch.Start();
            }

            return _pingPongWatch.ElapsedMilliseconds < Timeout;
        }

        private bool IsConnectionHealthy() {
            lock(StateLock) {
                bool isConnected = _tcpClient.Client.Connected && _clientConnected;
                bool isPingOk = PingPongCheck();

                return isConnected && isPingOk;
            }
        }

        public async Task HandleClient() {
            _sessionTimer.Restart();
            _clientConnected = true;
            _pongSignal = true;
            _pingPongWatch.Reset();

            try {
                ReceiveAsync();

                while(_clientConnected) {
                    _clientConnected = IsConnectionHealthy();

                    if(!_clientConnected)
                        break;

                    if(_responses.TryDequeue(out ResponseMessage response)) {
                        SendResponse(response);
                    }
                    else if(_requests.TryDequeue(out RpcRequest request)) {
                        await ProcessRequestAsync(request).ConfigureAwait(false);
                    }
                    else {
                        if(_sessionTimer.ElapsedMilliseconds > Timeout) {
                            _sessionTimer.Restart();
                            _pongSignal = false;
                        }

                        await Task.Delay(50).ConfigureAwait(false);
                    }
                }
            }
            catch(System.Exception ex) {
                Console.WriteLine($"Error occurred while handling client: {ex}");
                Logger.Exception(ex);
            }
            finally {
                Logger.Log($"Disconnect {((IPEndPoint)_tcpClient.Client.RemoteEndPoint).Address}");
                CloseClientConnection();
                CleanupUserSession();
                _sessionTimer.Stop();
            }
        }


        private async Task ReceiveAsync() {
            while(_clientConnected) {
                try {
                    var messageSize = await ReceiveMessageSizeAsync();

                    if(messageSize >= 100_000_000) {
                        Logger.Error("DDos Detected!!");
                        _clientConnected = false;
                        BlockIpAddress();
                        break;
                    }

                    if(messageSize > 0) {
                        var messageBytes = await ReceiveMessageBytesAsync(messageSize);

                        if(messageBytes.Length > 1) 
                            ProcessReceivedRequest(messageBytes);
                        else
                            SendPong();
                    }
                    else 
                        SendPong();

                    await Task.Delay(50);
                }
                catch(ReceiveException ex) {
                    Logger.Error($"ReceiveException: {ex.Message}");
                    _clientConnected = false;
                    break;
                }
                catch(ObjectDisposedException ex) {
                    Logger.Error($"ObjectDisposedException: {ex.Message}");
                    _clientConnected = false;
                    break;
                }
                catch(System.Exception ex) {
                    Logger.Error($"General Exception: {ex.Message}");
                    _clientConnected = false;
                    break;
                }
            }
        }


        private async Task<byte[]> ReceiveMessageBytesAsync(int messageSize) {
            var buffer = new byte[messageSize];
            var bytesRead = 0;

            while(bytesRead < messageSize) {
                int received = await _tcpClient.GetStream().ReadAsync(buffer, bytesRead, messageSize - bytesRead);
                if(received == 0) {
                    throw new ReceiveException();
                }
                bytesRead += received;
            }

            return buffer;
        }

        private async Task<int> ReceiveMessageSizeAsync() {
            var sizeBytes = new byte[4];

            for(int i = 0; i < sizeBytes.Length; ++i) {
                try {
                    sizeBytes[sizeBytes.Length - 1 - i] = await ReceiveByteAsync();
                }
                catch(ReceiveException) {
                    Logger.Error("ReceiveException occurred");
                    throw;
                }
            }

            int messageSize = BitConverter.ToInt32(sizeBytes, 0);
            Logger.Log($"Message size converted: {messageSize}");
            return messageSize;
        }

        private void BlockIpAddress() {
            try {
                var process = new Process();
                process.StartInfo.FileName = "iptables";
                process.StartInfo.Arguments = $"-A INPUT -s {((IPEndPoint)_tcpClient.Client.RemoteEndPoint).Address} -j DROP";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
            }
            catch(System.Exception ex) {
                Logger.Error($"Failed to block IP address: {ex.Message}");
            }
        }

        private async Task<byte> ReceiveByteAsync() {
            var buffer = new byte[1];
            var bytesRead = await _tcpClient.GetStream().ReadAsync(buffer, 0, 1);

            if(bytesRead is 1) return buffer[0];
            else throw new ReceiveException();
        }

        private void CloseClientConnection() {
            Logger.Log("Closing TcpClient");
            try {
                _tcpClient.GetStream().Close();
            }
            catch(System.Exception ex) {
                Logger.Log(ex.ToString());
            }
            finally {
                _tcpClient.Close();
            }
        }

        private void CleanupUserSession() {
            if(Statics.Users.Remove(_tcpClient, out string playerId)) {
                _sessionTimer.Stop();
            }
        }

        private void ProcessReceivedRequest(byte[] data) {
            try {
                var request = RpcRequest.Parser.ParseFrom(data);
               
                _requests.Enqueue(request);
            }
            catch(System.Exception ex) {
                Logger.Error($"Failed to parse RpcRequest: {ex}");
            }
        }

        public async void SendResponse(ResponseMessage response) {
            var messageBody = response.ToByteArray();
            var message = WrapMessage(messageBody);

            try {
                await _tcpClient.GetStream().WriteAsync(message, 0, message.Length).ConfigureAwait(false);
            }
            catch(System.Exception ex) {
                Logger.Error($"Failed to send response: {ex}");
            }
        }

        public async Task SendResponseAsync(ResponseMessage response) {
            var messageBody = response.ToByteArray();
            var message = WrapMessage(messageBody);

            try {
                await _tcpClient.GetStream().WriteAsync(message, 0, message.Length).ConfigureAwait(false);
                
                Logger.Log("Response sent successfully");
            }
            catch(System.Exception ex) {
                Logger.Error($"Failed to send response: {ex}");
            }
        }

        private byte[] WrapMessage(byte[] body) {
            var lengthBytes = BitConverter.GetBytes(body.Length);
            Array.Reverse(lengthBytes);

            var message = new byte[4 + body.Length];
            Buffer.BlockCopy(lengthBytes, 0, message, 0, 4);
            Buffer.BlockCopy(body, 0, message, 4, body.Length);

            return message;
        }

        private async Task ProcessRequestAsync(RpcRequest request) {
            try {
                Logger.Log($"Request from client. {request.ServiceName}.{request.MethodName}");
                if(_rpcHandlers.TryGetValue(request.ServiceName, out var handler)) {
                    if(IsAuthSkipped(request.ServiceName)) {
                    }
                    else if(Statics.Users.TryGetValue(_tcpClient, out var id) && _playerId == string.Empty) {
                        _playerId = id;
                    }
                    else {
                        Logger.Log("Failed get playerId"); // not auth
                   //     SendErrorResponseAsync(request.Id, 404);
                     //   return; 
                    }

                    try
                    {
                        await handler.InvokeAsync(request, _playerId);
                    }
                    catch (RpcException ex)
                    {
                        SendErrorResponseAsync(request.Id, ex.RpcCode);
                        return;
                    }
                    catch (System.Exception ex)
                    {
                        if (ex is RpcException) return;
                        
                        SendErrorResponseAsync(request.Id, 401);
                    }
                }
                else {
                    Logger.Log($"No handler found for service: {request.ServiceName}");
                    SendErrorResponseAsync(request.Id, 404);
                }
            }
            catch(System.Exception ex) {
                Logger.Exception(ex);
                SendErrorResponseAsync(request.Id, 500);
            }
        }

        private bool IsAuthSkipped(string serviceName) {
            return serviceName switch {
                "HandshakeRemoteService" or "AuthRemoteSerivce" => true,
                _ => false,
            };
        }

        private void SendErrorResponseAsync(string requestId, int errorCode) {
            var errorResponse = new ResponseMessage {
                RpcResponse = new RpcResponse {
                    Id = requestId,
                    Exception = new Exception {
                        Id = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 8),
                        Code = errorCode
                    }
                }
            };

            SendResponse(errorResponse);
        }

        private void SendPong() {
            byte[] pongMessage = WrapMessage(new byte[] { 1 });

            try {
                _tcpClient.GetStream().Write(pongMessage, 0, pongMessage.Length);
                
                Logger.Log("Pong sent successfully!");
                _pongSignal = true;
            }
            catch(System.Exception ex) {
                Logger.Error($"Failed to send Pong: {ex}");
            }
        }

        private class ReceiveException : System.Exception {
            public ReceiveException() : base("Failed to receive data.") { }
            public ReceiveException(string message) : base(message) { }
        }
    }
}

using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace ModalStrikeServer.RpcServer.Core {
    public class ClientHandler {
        private readonly TcpClient _client;

        private readonly Dictionary<string, RpcService> _services = new();

        private readonly ConcurrentQueue<byte[]> _sendQueue = new();

        private bool _isRunning = true;

        public ClientHandler(TcpClient client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            InitializeServices();
        }

        private void InitializeServices() {
           // _services.Add("echo", new EchoService());
        }

        public async Task HandleClientAsync() {
            Console.WriteLine($"Client connected: {_client.Client.RemoteEndPoint}");

            Task sendTask = Task.Run(() => SendDataAsync());

            try {
                // Читаем данные из сокета
                using(NetworkStream stream = _client.GetStream()) {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    while(_isRunning && (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Received: {receivedData}");

                        ProcessData(receivedData);
                    }
                }
            }
            catch(Exception ex) {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            finally {
                Console.WriteLine($"Client disconnected: {_client.Client.RemoteEndPoint}");
                _isRunning = false; // Останавливаем поток отправки
                _client.Close();
            }

            await sendTask; // Ждем завершения потока отправки
        }

        // Обрабатываем полученные данные и ставим ответ в очередь
        private void ProcessData(string data) {
            // Пример: разбиваем строку на команду и данные
            string[] parts = data.Split('|');
            if(parts.Length != 2) {
                EnqueueResponse("Invalid request format. Use 'command|data'.");
                return;
            }

            string command = parts[0].ToLower();
            string requestData = parts[1];

            // Создаем объект Request
          //  Request request = new Request { Method = command, Data = requestData };

            // Находим и вызываем соответствующий сервис
            if(_services.TryGetValue(command, out RpcService service)) {
              //  Response response = service.ProcessRequest(request);
               // EnqueueResponse($"{response.Status}|{response.Message}");
            }
            else {
                EnqueueResponse($"Unknown command: {command}");
            }
        }

        // Добавляем ответ в очередь для отправки
        private void EnqueueResponse(string message) {
            byte[] bytes = Encoding.UTF8.GetBytes(message + Environment.NewLine);
            _sendQueue.Enqueue(bytes);
        }

        // Асинхронно отправляем данные из очереди
        private async Task SendDataAsync() {
            while(_isRunning || !_sendQueue.IsEmpty) {
                if(_sendQueue.TryDequeue(out byte[] data)) {
                    try {
                        await _client.GetStream().WriteAsync(data, 0, data.Length);
                        Console.WriteLine($"Sent: {Encoding.UTF8.GetString(data)}");
                    }
                    catch(Exception ex) {
                        Console.WriteLine($"Send exception: {ex.Message}");
                        _isRunning = false; // Останавливаем отправку при ошибке
                    }
                }
                else {
                    await Task.Delay(50); // Ждем, если очередь пуста
                }
            }
        }
    }
}

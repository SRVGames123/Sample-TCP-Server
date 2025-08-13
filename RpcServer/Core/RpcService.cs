using ModalStrike.Protobuf2;
using ModalStrikeServer.MongoDb;
using ModalStrikeServer.MongoDb.Core;
using ModalStrikeServer.RpcServer.Utilities;
using ModalStrikeServer.RpcServer.Utilities.CustomLogger;
using MongoDB.Bson;

namespace ModalStrikeServer.RpcServer.Core {
    public abstract class RpcService {
        protected TCPServer TCPServer;

        protected string PlayerId = string.Empty;

        protected ObjectId PlayerObjectId => ObjectId.Parse(PlayerId);

        private string requestId = string.Empty;

        private RpcRequest _rpcRequest;

        protected ServiceLogger Logger;

        public RpcService(TCPServer server, ServiceLogger logger) {
            TCPServer = server;
            Logger = logger;
        } 

        public async Task InvokeAsync(RpcRequest request, string playerId) {
            requestId = request.Id;
            _rpcRequest = request;
            PlayerId = playerId;

            await Invoke(request);
        }

        protected abstract Task Invoke(RpcRequest request);
        
        protected void MethodNotFound() => Logger.Error($"Method from {_rpcRequest.MethodName} not found!");

        #region SendResponse

        protected async Task ReturnEmptyAsync()
            => await TCPServer.SendResponseAsync(ResponseHelper.CreateResponse(requestId));

        protected async Task ReturnValueAsync<T>(T value)
            => await TCPServer.SendResponseAsync(ResponseHelper.CreateResponse(requestId, new ToByteMethod(typeof(T)).ToBytes(value)));

        protected async Task ReturnErrorAsync(int code)
            => await TCPServer.SendResponseAsync(ResponseHelper.CreateResponse(requestId, code));

        #endregion
    }
}

public class ResponseHelper {
    public static ResponseMessage CreateResponse(string requestId, BinaryValue value = null) {
        var response = new ResponseMessage() {
            RpcResponse = new RpcResponse {
                Id = requestId,
                Return = value,
            }
        };

        return response;
    }

    public static ResponseMessage CreateResponse(string requestId, int code) {
        var response = new ResponseMessage() {
            RpcResponse = new RpcResponse {
                Id = requestId,
                Exception = new ModalStrike.Protobuf2.Exception {
                    Id = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 8),
                    Code = code
                }
            }
        };

        return response;
    }

    public static ResponseMessage CreateResponse(string requestId) {
        var response = new ResponseMessage() {
            RpcResponse = new RpcResponse {
                Id = requestId,
                Return = new BinaryValue {
                    IsNull = true,
                }
            }
        };

        return response;
    }
}

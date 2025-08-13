using ModalStrike.Protobuf2;
using ModalStrikeServer.RpcServer.Core;
using ModalStrikeServer.RpcServer.Events.Core;

namespace ModalStrikeServer.RpcServer.Events {
    public class MessageEventSender : IEventService {
        public string NameEventService => "MessageEventSender";

        private readonly TCPServer TcpServer;

        public MessageEventSender(TCPServer server) => TcpServer = server;

        public async Task SendMessage(string pattern) {
            var responseMessage = new ResponseMessage {
                EventResponse = new EventResponse {
                    EventName = "OnMessage",
                    ListenerName = NameEventService
                }
            };

            responseMessage.EventResponse.Params.Add(ProtoReflectionUtils.CreateToByteMethod(typeof(string)).ToBytes(pattern));

            await TcpServer.SendResponseAsync(responseMessage);
        }

        public async Task SendEvent(string name, object[] paramsEvent) {
            if(name == "SendMessage") {
                await SendMessage((string)paramsEvent[0]);
            }
        }
    }
}

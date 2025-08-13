using ModalStrike.Protobuf2;
using ModalStrikeServer.RpcServer.Core;
using ModalStrikeServer.RpcServer.Events.Core;

namespace ModalStrikeServer.RpcServer.Events
{
    public class PlayerEventService : IEventService
    {
        public string NameEventService => "playerEventService";
        
        private readonly TCPServer TcpServer;

        public PlayerEventService(TCPServer server) => TcpServer = server;
        
        public async Task PlayerBanned() {        
            var responseMessage = new ResponseMessage {
                EventResponse = new EventResponse {
                    EventName = "playerBanned",
                    ListenerName = NameEventService
                }
            };
            
            await TcpServer.SendResponseAsync(responseMessage);
        }

        public async Task SendEvent(string name, object[] paramsEvent) {
        }
    }
}
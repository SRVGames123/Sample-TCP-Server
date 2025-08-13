using ModalStrike.Protobuf2;
using ModalStrikeServer.RpcServer.Core;
using ModalStrikeServer.RpcServer.Extensions;
using ModalStrikeServer.RpcServer.Utilities.CustomLogger;

namespace ModalStrikeServer.RpcServer.Services
{
    public class HandshakeRemoteService(TCPServer user, ServiceLogger logger) : RpcService(user, logger) {
        public async Task Handshake(BinaryValue[] values) {
            var token = values.GetValue<string>(0);

            if(Statics.PlayerTokens.TryGetValue(token, out var playerId)) {
                Statics.Users.Add(TCPServer.TcpClient, playerId);

                await ReturnEmptyAsync();
                return;
            }

            await ReturnErrorAsync(401);
        }

        protected override async Task Invoke(RpcRequest request)
        {
            switch(request.MethodName) {
                case "Handshake":
                    await Handshake(request.Params.ToArray());
                    break;
                default:
                    MethodNotFound();
                    break;
            }
        }
    }
}
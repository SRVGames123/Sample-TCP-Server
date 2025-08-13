using ModalStrike.Protobuf2;
using ModalStrikeServer.MongoDb.Core;
using ModalStrikeServer.RpcServer.Core;
using ModalStrikeServer.RpcServer.Extensions;
using ModalStrikeServer.RpcServer.Extensions.Player;
using ModalStrikeServer.RpcServer.Utilities.CustomLogger;

namespace ModalStrikeServer.RpcServer.Services
{
    public class PlayerRemoteService(TCPServer user, ServiceLogger logger) : RpcService(user, logger) {
        public async Task GetLocalPlayer()
        {
            Console.WriteLine(PlayerObjectId);
            var playerModel = await UnitOfWork.Players.GetByIdAsync(PlayerObjectId);
            var player = playerModel.ToProto();
            
            await ReturnValueAsync(player);
        }

        public async Task RenamePlayer(BinaryValue[] values)
        {
            var newName = values.GetValue<string>(0);

            await UnitOfWork.Players.RenamePlayerAsync(PlayerObjectId, newName);
            await ReturnEmptyAsync();
        }

        public async Task BanMe(BinaryValue[] values)
        {
            var reason = values.GetValue<string>(0);
            
            await UnitOfWork.Players.BanAsync(PlayerObjectId, reason);
            
            await ReturnEmptyAsync();
        }

        protected override async Task Invoke(RpcRequest request)
        {
            switch (request.MethodName)
            {
                case "GetLocalPlayer":
                    await GetLocalPlayer();
                    break;
                case "RenamePlayer":
                    await RenamePlayer(request.Params.ToArray());
                    break;
                case "BanMe":
                    await BanMe(request.Params.ToArray());
                    break;
                default:
                    MethodNotFound();
                    throw new NotImplementedException();
                    break;
            }
        }
    }
}
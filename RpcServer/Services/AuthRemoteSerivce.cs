using System.Net;
using ModalStrike.Protobuf2;
using ModalStrikeServer.MongoDb.Core;
using ModalStrikeServer.RpcServer.Core;
using ModalStrikeServer.RpcServer.Exceptions.Account;
using ModalStrikeServer.RpcServer.Extensions;
using ModalStrikeServer.RpcServer.Extensions.Player;
using ModalStrikeServer.RpcServer.Utilities;
using ModalStrikeServer.RpcServer.Utilities.CustomLogger;
using MongoDB.Bson;

namespace ModalStrikeServer.RpcServer.Services {
    public class AuthRemoteSerivce(TCPServer user, ServiceLogger logger) : RpcService(user, logger) {

        public async Task Auth(BinaryValue[] values) {
            var login = EncryptUtility.MD5(values.GetValue<string>(0));
            var password = EncryptUtility.MD5(values.GetValue<string>(1));

            var result = await UnitOfWork.Accounts.IsValidAccountAsync(login, password);

            var userIp = ((IPEndPoint)TCPServer.TcpClient.Client.RemoteEndPoint).Address.ToString();
      
            switch (result)
            {
                case ResultAccountAuth.SuccessfullyAuth:
                    var accountModel = await UnitOfWork.Accounts
                        .AuthenticateAccountAsync(login, password);

                    var token = TokenExtension.CreateNewTicket(accountModel.playerId);

                    Statics.PlayerTokens.TryAdd(token, accountModel.playerId);

                    await ReturnValueAsync<string>(token);
                    break;
                case ResultAccountAuth.NotAuthorized:
                    var playerModel = PlayerExtension.CreateNewPlayer(userIp);
            
                    var playerId = await UnitOfWork.Players.CreateAsync(playerModel);
                    var account = PlayerExtension.CreateAccount(login, password, playerId);
            
                    var model = await UnitOfWork.Accounts.CreateAccountAsync(account);

                    var tokenUser = TokenExtension.CreateNewTicket(account.playerId);

                    Statics.PlayerTokens.TryAdd(tokenUser, account.playerId);

                    await ReturnValueAsync<string>(tokenUser);
                    break;
                case ResultAccountAuth.IncorrectPassword:
                    await ReturnErrorAsync(1002);
                    break;
                case ResultAccountAuth.InvalidLogin:
                    await ReturnErrorAsync(1001);
                    break;
                case ResultAccountAuth.InvalidPassword:
                    await ReturnErrorAsync(1003);
                    break;
            }
        }

        protected override async Task Invoke(RpcRequest request) {
            switch(request.MethodName) {
                case "Auth":
                    await Auth(request.Params.ToArray());
                    break;
                default:
                    MethodNotFound();
                    break;
            }
        }
    }
}

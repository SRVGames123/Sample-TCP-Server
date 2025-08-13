using ModalStrike.Protobuf2;
using ModalStrikeServer.MongoDb.Model.Account;
using ModalStrikeServer.MongoDb.Model.Player;
using MongoDB.Bson;

namespace ModalStrikeServer.RpcServer.Extensions.Player
{
    public static class PlayerExtension
    {
        public static PlayerModel CreateNewPlayer(string ip)
        {
            var playerData = new BsonDocument
            {
                { "registrationIp", ip },
                { "usedIps", new BsonArray { ip } } 
            };
            
            return new()
            {
                Id = ObjectId.GenerateNewId(),
                name = $"Player_{new Random().Next(0, 99999)}",
                avatar = "",
                registrationData = DateTime.Now,
                banned = false,
                bannedReason = ""
            };
        }

        public static ModalStrike.Protobuf2.Player ToProto(this PlayerModel player)
        {
            return new()
            {
                Id = player.Id.ToString(),
                Name = player.name,
                Uid = player.uid,
                AvatarId = player.avatar,
                OnlineStatus = OnlineStatus.StateOnline,
            };
        }
        
        public static AccountModel CreateAccount(string login, string password, ObjectId playerId)
         => CreateAccount(login, password, playerId.ToString());
        
        public static AccountModel CreateAccount(string login, string password, string playerId)
        {
            return new()
            {
                login = login,
                password = password,
                playerId = playerId,
            };
        }
    }
}
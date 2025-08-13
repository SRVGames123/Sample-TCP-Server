using ModalStrikeServer.MongoDb.Core;
using MongoDB.Bson;

namespace ModalStrikeServer.MongoDb.Model.Account {
    public class AccountModel : ModelBase {
        public string login { get; set; }
        public string password { get; set; }
        public string playerId { get; set; }
        public BsonDocument userData { get; set; }
    }
}

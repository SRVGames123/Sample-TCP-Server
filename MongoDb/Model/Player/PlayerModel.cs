using ModalStrikeServer.MongoDb.Core;
using MongoDB.Bson;

namespace ModalStrikeServer.MongoDb.Model.Player {
    public class PlayerModel : ModelBase {
        public string uid {  get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        public BsonDateTime registrationData { get; set; }
        public bool banned { get; set; }
        public string bannedReason { get; set; }
        public BsonDocument playerData { get; set; } 
    }
}

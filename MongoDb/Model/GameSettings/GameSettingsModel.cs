using ModalStrikeServer.MongoDb.Core;
using MongoDB.Bson;

namespace ModalStrikeServer.MongoDb.Model.GameSettings
{
    public class GameSettingsModel : ModelBase
    {
        public string currentVersion { get; set; }
        public string apkHash { get; set; }
        public BsonArray photonAppIds { get; set; }
    }
}
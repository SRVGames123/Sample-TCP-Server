using ModalStrikeServer.MongoDb.Core;
using MongoDB.Bson;

namespace ModalStrikeServer.MongoDb.Model.Stats
{
    public class StatsModel : ModelBase
    {
        public string playerId { get; set; }
        public BsonDocument stats { get; set; }
    }
}
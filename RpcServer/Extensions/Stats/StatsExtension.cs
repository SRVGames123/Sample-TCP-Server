using ModalStrikeServer.MongoDb.Model.Stats;
using MongoDB.Bson;

namespace ModalStrikeServer.RpcServer.Extensions.Stats
{
    public static class StatsExtension
    {
        private static readonly string[] GameModes = { "DeathMatch", "ArmsRace" };
        private static readonly string[] Stats = { "Kills", "Death", "Assist" };

        public static StatsModel CreateStats(string playerId)
        {
            var statElements = GenerateStatElements();
            
            return new StatsModel
            {
                playerId = playerId,
                stats = new BsonDocument(statElements)
            };
        }

        private static List<BsonElement> GenerateStatElements()
        {
            var statElements = new List<BsonElement>();
            
            foreach (var gameMode in GameModes)
            {
                foreach (var stat in Stats)
                {
                    var statName = $"{gameMode}_{stat}";
                    statElements.Add(new BsonElement(statName, 0));
                }
            }
            
            return statElements;
        }
    }
}
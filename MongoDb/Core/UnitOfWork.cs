using ModalStrikeServer.MongoDb.Repositories;
using ModalStrikeServer.RpcServer.Utilities;
using MongoDB.Driver;

namespace ModalStrikeServer.MongoDb.Core
{
    public static class UnitOfWork
    {
        private static readonly string DatabaseName ="ModalStrike";
        
        private static IMongoDatabase _database;
        
        private static MongoClient _client;
        
        public static AccountRepository Accounts => new(_database);
        public static PlayerRepository Players => new(_database);

        public static void Initialize()
        {
            _client = new MongoClient("mongodb://localhost:2077");
            _database = _client.GetDatabase(DatabaseName);
            
            if(_client is not null && _database is not null)  Logger.Debug("Successfully connected to MongoDB");
        }
    }
}
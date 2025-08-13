using MongoDB.Driver;
using System;

namespace ModalStrikeServer.MongoDb.Core {
    public class MongoDbSettings {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; } = "ModalStrike";
    }

    public abstract class DatabaseBase : IDisposable {
        protected IMongoDatabase Database { get; }
        private MongoClient _client;
        private bool _disposed;

        protected DatabaseBase(MongoDbSettings settings) {
            if(settings == null) throw new ArgumentNullException(nameof(settings));
            if(string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ArgumentException("MongoDB connection string cannot be empty");

            _client = new MongoClient(settings.ConnectionString);
            Database = _client.GetDatabase(settings.DatabaseName);
        }

        protected DatabaseBase(string connectionString, string databaseName = null) {
            if(string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

            _client = new MongoClient(connectionString);
            Database = _client.GetDatabase(databaseName ?? "ModalStrike");
        }

        protected virtual void InitializeCollections() {

        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if(_disposed) return;

            if(disposing) {
                _client = null;
            }

            _disposed = true;
        }

       /* ~DatabaseBase() {
            Dispose(false);
        }*/
    }
}
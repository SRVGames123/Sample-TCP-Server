using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ModalStrikeServer.MongoDb.Core {
    [Serializable]
    public abstract class ModelBase {
        [BsonId]
        public ObjectId _id { get; set; }
        
        [BsonIgnore]
        public ObjectId Id
        {
            get => _id;
            set => _id = value;
        }
    }
}
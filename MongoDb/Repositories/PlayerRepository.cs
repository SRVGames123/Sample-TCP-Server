using ModalStrikeServer.MongoDb.Core;
using ModalStrikeServer.MongoDb.Model.Player;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ModalStrikeServer.MongoDb.Repositories
{
    public class PlayerRepository(IMongoDatabase database) : RepositoryBase<PlayerModel>(database)
    {
        protected override string CollectionName =>"players";

        public async Task<ObjectId> CreateAsync(PlayerModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            model.uid = await GetLastUid();

            await CreateAsync(model);
            
            return model.Id;
        }

        public async Task<PlayerModel> GetByIdAsync(ObjectId playerId) 
            => await GetAsync(x => x._id == playerId);

        public async Task<PlayerModel> GetByUidAsync(string uid)
            => await GetAsync(x => x.uid == uid);

        public async Task BanAsync(ObjectId playerId, string reason)
        {
            var updateResult = await UpdateAsync(
                x => x.Id == playerId,
                UpdateBuilder
                    .Set(x => x.banned, true)
                    .Set(x => x.bannedReason, reason));
        }

        public async Task RenamePlayerAsync(ObjectId playerId, string newName)
        {
            var filter = FilterBuilder.Eq(x => x._id, playerId);

            var update = UpdateBuilder.Set(x => x.name, newName);

            await UpdateAsync(filter, update);
        }

        public async Task AddIpUserAsync(ObjectId playerId, string ipAddress)
        {
            var filter = FilterBuilder.Eq(x => x._id, playerId);

            var model = await GetAsync(filter);

            if (!model.playerData["usedIps"].AsBsonArray.Contains(ipAddress))
            { 
                var update = UpdateBuilder.Push("playerData.usedIps", ipAddress);
                
                await UpdateAsync(filter, update);
            }
        }

        public async Task<string> GetLastUid() {
            var lastDocument = await Collection.Find(new BsonDocument())
              .Sort(new BsonDocument("_id", -1))
              .Limit(1)
              .FirstOrDefaultAsync();

            return lastDocument != null ? $"{int.Parse(lastDocument.uid) + 1}" : "1";
        }
    }
}
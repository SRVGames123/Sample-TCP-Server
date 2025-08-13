using ModalStrikeServer.MongoDb.Core;
using ModalStrikeServer.MongoDb.Model.GameSettings;
using MongoDB.Driver;

namespace ModalStrikeServer.MongoDb.Repositories
{
    public class GameSettingsRepository(IMongoDatabase database) : RepositoryBase<GameSettingsModel>(database)
    {
        protected override string CollectionName => "gameSettings";

        public async Task<GameSettingsModel> GetGameSettingsAsync()
            => await GetAsync(FilterBuilder.Empty);
    }
}
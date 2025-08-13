using ModalStrikeServer.MongoDb.Core;
using ModalStrikeServer.MongoDb.Model.Account;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ModalStrikeServer.MongoDb.Repositories {
    public class AccountRepository(IMongoDatabase database) : RepositoryBase<AccountModel>(database) {
        protected override string CollectionName => "accounts";
        
        public async Task<ObjectId> CreateAccountAsync(AccountModel model) {
            await CreateAsync(model);
            return model.Id;
        }

        public async Task<AccountModel> AuthenticateAccountAsync(string login, string password) {
            var filter = Builders<AccountModel>.Filter.And(
                Builders<AccountModel>.Filter.Eq(x => x.login, login),
                Builders<AccountModel>.Filter.Eq(x => x.password, password)
            );

            return await GetAsync(filter);
        }

        public async Task<bool> IsAccountUniqueAsync(AccountModel model) {
            var filter = Builders<AccountModel>.Filter.And(
                Builders<AccountModel>.Filter.Eq(x => x.playerId, model.playerId),
                Builders<AccountModel>.Filter.Eq(x => x.login, model.login)
            );

            return await CountAsync(filter) == 0;
        }

        public async Task<ResultAccountAuth> IsValidAccountAsync(string login, string password)
        {
            if (login.Length <= 5)
                return ResultAccountAuth.InvalidLogin;
            else if(password.Length < 8)
                return ResultAccountAuth.InvalidPassword;
                
            var filter = Builders<AccountModel>.Filter.Eq(x => x.login, login);
            var account = await GetAsync(filter);   

            if (account is null) return ResultAccountAuth.NotAuthorized;

            return password == account.password
                ? ResultAccountAuth.SuccessfullyAuth
                : ResultAccountAuth.IncorrectPassword;
        }
    }
}

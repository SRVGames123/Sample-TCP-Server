using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;


namespace ModalStrikeServer.MongoDb.Core;

  public abstract class RepositoryBase<TModel>(IMongoDatabase database) where TModel : ModelBase {
        private IMongoCollection<TModel>? _collection;
        protected abstract string CollectionName { get; }
        public IMongoCollection<TModel> Collection => _collection ??= database.GetCollection<TModel>(CollectionName);

        protected static FilterDefinitionBuilder<TModel> FilterBuilder => Builders<TModel>.Filter;
        protected static SortDefinitionBuilder<TModel> SortBuilder => Builders<TModel>.Sort;
        protected static UpdateDefinitionBuilder<TModel> UpdateBuilder => Builders<TModel>.Update;
        protected static UpdateDefinition<TModel> CombinedUpdateBuilder => Builders<TModel>.Update.Combine();

        public virtual async Task<TModel> CreateAsync(TModel model) {
            await Collection.InsertOneAsync(model);
            return model;
        }

        public virtual async Task<TModel[]> CreateAsync(IEnumerable<TModel> models) {
            var insertManyAsync = models as TModel[] ?? models.ToArray();

            await Collection.InsertManyAsync(insertManyAsync);
            return insertManyAsync;
        }

        public virtual async Task<bool> ExistsAsync(ObjectId id) {
            return await Collection.Find(model => model.Id == id).AnyAsync();
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TModel, bool>> expression) {
            return await Collection.Find(expression).AnyAsync();
        }

        public virtual async Task<bool> ExistsAsync(FilterDefinition<TModel> filter) {
            return await Collection.Find(filter).AnyAsync();
        }

        public IFindFluent<TModel, TModel> GetFluent(Expression<Func<TModel, bool>> expression) {
            return Collection.Find(expression);
        }

        public virtual async Task<TModel> GetFirstAsync() {
            return await Collection.Find(FilterDefinition<TModel>.Empty).FirstOrDefaultAsync();
        }

        public virtual async Task<TModel> GetAsync(string stringId) {
            return await Collection.Find(model => model.Id == ObjectId.Parse(stringId)).FirstOrDefaultAsync();
        }

        public virtual async Task<TModel> GetAsync(ObjectId id) {
            return await Collection.Find(model => model.Id == id).FirstOrDefaultAsync();
        }

        public virtual async Task<TModel> GetAsync(Expression<Func<TModel, bool>> expression) {
            return await Collection.Find(expression).FirstOrDefaultAsync();
        }

        public virtual async Task<TModel> GetAsync(FilterDefinition<TModel> filter) {
            return await Collection.Find(filter).FirstOrDefaultAsync();
        }

        public virtual async Task<TModel> FindOneAndUpdateAsync(ObjectId id, UpdateDefinition<TModel> update, FindOneAndUpdateOptions<TModel>? options = null) {
            return await Collection.FindOneAndUpdateAsync<TModel>(model => model.Id == id, update,
                options ?? new FindOneAndUpdateOptions<TModel> { ReturnDocument = ReturnDocument.After });
        }

        public virtual async Task<TModel> FindOneAndUpdateAsync(FilterDefinition<TModel> filter,
            UpdateDefinition<TModel> update,
            FindOneAndUpdateOptions<TModel>? options = null) {
            return await Collection.FindOneAndUpdateAsync(filter, update,
                options ?? new FindOneAndUpdateOptions<TModel> { ReturnDocument = ReturnDocument.After });
        }

        public virtual async Task<TModel?> FindOneAndUpdateAsync(Expression<Func<TModel, bool>> expression,
            UpdateDefinition<TModel> update,
            FindOneAndUpdateOptions<TModel>? options = null) {
            return await Collection.FindOneAndUpdateAsync(expression, update,
                options ?? new FindOneAndUpdateOptions<TModel> { ReturnDocument = ReturnDocument.After });
        }

        public virtual async Task<List<TModel>> GetManyAsync(ObjectId id, FindOptions? options = null) {
            return await Collection.Find(model => model.Id == id, options).ToListAsync();
        }

        public virtual async Task<List<TModel>> GetManyAsync(Expression<Func<TModel, bool>> expression,
            FindOptions? options = null) {
            return await Collection.Find(expression, options).ToListAsync();
        }

        public virtual async Task<List<TModel>> GetManyAsync(FilterDefinition<TModel> filter, FindOptions? options = null) {
            return await Collection.Find(filter, options).ToListAsync();
        }

        public virtual async Task<List<TModel>> GetAllAsync() {
            return await Collection.Find(FilterDefinition<TModel>.Empty).ToListAsync();
        }

        public virtual async Task<IEnumerable<TModel>> GetBatchAsync(ObjectId id, int page, int size,
            FindOptions? options = null) {
            return (await Collection.Find(model => model.Id == id, options).ToListAsync()).Skip(page * size).Take(size);
        }

        public virtual async Task<IEnumerable<TModel>> GetBatchAsync(Expression<Func<TModel, bool>> expression, int page,
            int size, FindOptions? options = null) {
            return await Collection.Find(expression, options).Skip(page * size).Limit(size).ToListAsync();
        }

        public virtual async Task<List<TModel>> GetBatchAsync(FilterDefinition<TModel> filter, int page, int size,
            FindOptions? options = null) {
            return await Collection.Find(filter, options).Skip(page * size).Limit(size).ToListAsync();
        }

        public virtual async Task<TProjection> GetProjectedAsync<TProjection>(ObjectId id,
            Expression<Func<TModel, TProjection>> projection) {
            return await Collection.Find(model => model.Id == id).Project(projection).FirstOrDefaultAsync();
        }

        public virtual async Task<TProjection> GetProjectedAsync<TProjection>(Expression<Func<TModel, bool>> expression,
            Expression<Func<TModel, TProjection>> projection) {
            return await Collection.Find(expression).Project(projection).FirstOrDefaultAsync();
        }

        public virtual async Task<TProjection> GetProjectedAsync<TProjection>(FilterDefinition<TModel> filter,
            Expression<Func<TModel, TProjection>> projection) {
            return await Collection.Find(filter).Project(projection).FirstOrDefaultAsync();
        }

        public async Task<List<TProjection>> GetProjectedLimitedAsync<TProjection>(
            Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TProjection>> projection, int limit) {
            return await Collection.Find(expression).Project(projection).Limit(limit).ToListAsync();
        }

        public virtual async Task<List<TProjection>> GetProjectedManyAsync<TProjection>(ObjectId id,
            Expression<Func<TModel, TProjection>> projection) {
            return await Collection.Find(model => model.Id == id).Project(projection).ToListAsync();
        }

        public virtual async Task<List<TProjection>> GetProjectedManyAsync<TProjection>(
            Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TProjection>> projection) {
            return await Collection.Find(expression).Project(projection).ToListAsync();
        }

        public virtual async Task<List<TProjection>> GetProjectedManyAsync<TProjection>(FilterDefinition<TModel> filter,
            Expression<Func<TModel, TProjection>> projection) {
            return await Collection.Find(filter).Project(projection).ToListAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<TModel, bool>> expression, CountOptions? options = null) {
            return (int)await Collection.CountDocumentsAsync(expression, options);
        }

        public virtual async Task<long> CountAsync(FilterDefinition<TModel> filter, CountOptions? options = null) {
            return await Collection.CountDocumentsAsync(filter, options);
        }

        public virtual async Task<ReplaceOneResult> ReplaceAsync(TModel replace,
            ReplaceOptions? options = null) {
            return await Collection.ReplaceOneAsync(model => model.Id == replace.Id, replace, options);
        }

        public virtual async Task<ReplaceOneResult> ReplaceAsync(Expression<Func<TModel, bool>> expression, TModel replace,
            ReplaceOptions? options = null) {
            return await Collection.ReplaceOneAsync(expression, replace, options);
        }

        public virtual async Task<ReplaceOneResult> ReplaceAsync(FilterDefinition<TModel> filter, TModel replace,
            ReplaceOptions? options = null) {
            return await Collection.ReplaceOneAsync(filter, replace, options);
        }

        public virtual async Task<UpdateResult> UpdateAsync(ObjectId id, UpdateDefinition<TModel> update,
            UpdateOptions? options = null) {
            return await Collection.UpdateOneAsync(model => model.Id == id, update, options);
        }

        public virtual async Task<UpdateResult> UpdateAsync(Expression<Func<TModel, bool>> expression,
            UpdateDefinition<TModel> update,
            UpdateOptions? options = null) {
            return await Collection.UpdateOneAsync(expression, update, options);
        }

        public virtual async Task<UpdateResult> UpdateAsync(FilterDefinition<TModel> filter,
            UpdateDefinition<TModel> update,
            UpdateOptions? options = null) {
            return await Collection.UpdateOneAsync(filter, update, options);
        }


        public virtual async Task UpdateOrCreateAsync(ObjectId id, UpdateDefinition<TModel> update) {
            await Collection.UpdateOneAsync(model => model.Id == id, update, new UpdateOptions { IsUpsert = true });
        }

        public virtual async Task<UpdateResult> UpdateManyAsync(Expression<Func<TModel, bool>> expression,
            UpdateDefinition<TModel> update,
            UpdateOptions? options = null) {
            return await Collection.UpdateManyAsync(expression, update, options);
        }

        public virtual async Task<UpdateResult> UpdateManyAsync(FilterDefinition<TModel> filter,
            UpdateDefinition<TModel> update,
            UpdateOptions? options = null) {
            return await Collection.UpdateManyAsync(filter, update, options);
        }

        public virtual async Task<DeleteResult> DeleteAsync(TModel model) {
            return await Collection.DeleteOneAsync(model1 => model1.Id == model.Id);
        }

        public virtual async Task<DeleteResult> DeleteAsync(ObjectId id) {
            return await Collection.DeleteOneAsync(model => model.Id == id);
        }

        public virtual async Task<DeleteResult> DeleteAsync(Expression<Func<TModel, bool>> expression) {
            return await Collection.DeleteOneAsync(expression);
        }

        public virtual async Task<DeleteResult> DeleteAsync(FilterDefinition<TModel> filter) {
            return await Collection.DeleteOneAsync(filter);
        }

        /*public virtual async Task DeleteIfNullAsync(ObjectId id)
        {
            if (!await ExistsAsync(id)) await Collection.DeleteOneAsync(model => model.Id == id);
        }

        public virtual async Task DeleteIfNullAsync(Expression<Func<TModel, bool>> expression)
        {
            if (!await ExistsAsync(expression)) await Collection.DeleteOneAsync(expression);
        }

        public virtual async Task DeleteIfNullAsync(FilterDefinition<TModel> filter)
        {
            if (!await ExistsAsync(filter)) await Collection.DeleteOneAsync(filter);
        }*/

        public virtual async Task<DeleteResult> DeleteManyAsync(Expression<Func<TModel, bool>> expression) {
            return await Collection.DeleteManyAsync(expression);
        }

        public virtual async Task<DeleteResult> DeleteManyAsync(FilterDefinition<TModel> filter) {
            return await Collection.DeleteManyAsync(filter);
        }

        public virtual async Task CreateIndexAsync(IndexKeysDefinition<TModel> keys, CreateIndexOptions? options = null) {
            await Collection.Indexes.CreateOneAsync(new CreateIndexModel<TModel>(keys, options));
        }

        public virtual async Task DropIndexAsync(string name) {
            await Collection.Indexes.DropOneAsync(name);
        }
    }

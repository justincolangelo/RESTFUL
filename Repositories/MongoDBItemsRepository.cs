using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RESTFUL.Interfaces;
using RESTFUL.Entities;
using MongoDB.Driver;
using MongoDB.Bson;

namespace RESTFUL.Repositories
{
    public class MongoDBItemsRepository : IItemsRepository
    {
        private const string dbName = "RESTFUL";
        private const string dbCollection = "items";

        private readonly IMongoCollection<Item> _collection;
        private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        public MongoDBItemsRepository(IMongoClient client)
        {
            IMongoDatabase db = client.GetDatabase(dbName);
            _collection = db.GetCollection<Item>(dbCollection);
        }

        public async Task<IEnumerable<Item>> GetItemsAsync()
        {
            return await _collection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<Item> GetItemAsync(Guid id)
        {
            var filter = filterBuilder.Eq(x => x.Id, id);
            return await _collection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task CreateItemAsync(Item item)
        {
            await _collection.InsertOneAsync(item);
        }

        public async Task UpdateItemAsync(Item item)
        {
            var filter = filterBuilder.Eq(x => x.Id, item.Id);
            await _collection.ReplaceOneAsync(filter, item);
        }

        public async Task DeleteItemAsync(Guid id)
        {
            var filter = filterBuilder.Eq(x => x.Id, id);
            await _collection.DeleteOneAsync(filter);
        }
    }
}

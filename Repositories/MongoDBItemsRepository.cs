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
    public class MongoDBItemsRepository : IInMemoryItemsRepository
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

        public IEnumerable<Item> GetItems()
        {
            return _collection.Find(new BsonDocument()).ToList();
        }

        public Item GetItem(Guid id)
        {
            var filter = filterBuilder.Eq(x => x.Id, id);
            return _collection.Find(filter).SingleOrDefault();
        }

        public void CreateItem(Item item)
        {
            _collection.InsertOne(item);
        }

        public void UpdateItem(Item item)
        {
            var filter = filterBuilder.Eq(x => x.Id, item.Id);
            _collection.ReplaceOne(filter, item);
        }

        public void DeleteItem(Guid id)
        {
            var filter = filterBuilder.Eq(x => x.Id, id);
            _collection.DeleteOne(filter);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RESTFUL.Entities;
using RESTFUL.Interfaces;

namespace RESTFUL.Repositories
{
    public class InMemoryItemsRepository : IItemsRepository
    {
        // instance of the list should not change after construction
        private readonly List<Item> items = new ()
        { // as of C# 9 new() is possible
            new Item { Id = Guid.NewGuid(), Name = "Electric Guitar", Price = 100, CreatedDate = DateTime.UtcNow },
            new Item { Id = Guid.NewGuid(), Name = "Acoustic Guitar", Price = 90, CreatedDate = DateTime.UtcNow },
            new Item { Id = Guid.NewGuid(), Name = "Bass Electric Guitar", Price = 80, CreatedDate = DateTime.UtcNow }
        }; 

        public async Task<IEnumerable<Item>> GetItemsAsync()
        {
            return await Task.FromResult(items);
        }

        public async Task<Item> GetItemAsync(Guid id)
        {
            return await Task.FromResult(items.SingleOrDefault(x => x.Id == id));
        }

        public async Task CreateItemAsync(Item item)
        {
            items.Add(item);
            await Task.CompletedTask;
        }

        public async Task UpdateItemAsync(Item item)
        {
            var itemIndex = items.FindIndex(x => x.Id == item.Id);
            items[itemIndex] = item;
            await Task.CompletedTask;
        }

        public async Task DeleteItemAsync(Guid id)
        {
            var itemIndex = items.FindIndex(x => x.Id == id);
            items.RemoveAt(itemIndex);
            await Task.CompletedTask;
        }
    }
}

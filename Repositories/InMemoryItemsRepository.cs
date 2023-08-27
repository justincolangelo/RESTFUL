using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RESTFUL.Entities;
using RESTFUL.Interfaces;

namespace RESTFUL.Repositories
{
    public class InMemoryItemsRepository : IInMemoryItemsRepository
    {
        // instance of the list should not change after construction
        private readonly List<Item> items = new()
        { // as of C# 9 new() is possible
            new Item { Id = Guid.NewGuid(), Name = "Electric Guitar", Price = 100, CreatedDate = DateTimeOffset.UtcNow },
            new Item { Id = Guid.NewGuid(), Name = "Acoustic Guitar", Price = 90, CreatedDate = DateTimeOffset.UtcNow },
            new Item { Id = Guid.NewGuid(), Name = "Bass Electric Guitar", Price = 80, CreatedDate = DateTimeOffset.UtcNow }
        }; 

        public IEnumerable<Item> GetItems()
        {
            return items;
        }

        public Item GetItem(Guid id)
        {
            return items.SingleOrDefault(x => x.Id == id);
        }

        public void CreateItem(Item item)
        {
            items.Add(item);
        }

        public void UpdateItem(Item item)
        {
            var itemIndex = items.FindIndex(x => x.Id == item.Id);
            items[itemIndex] = item;
        }

        public void DeleteItem(Guid id)
        {
            var itemIndex = items.FindIndex(x => x.Id == id);
            items.RemoveAt(itemIndex);
        }
    }
}

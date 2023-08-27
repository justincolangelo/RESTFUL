using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RESTFUL.Entities;

namespace RESTFUL.Interfaces
{
    public interface IItemsRepository
    {
        public Task<IEnumerable<Item>> GetItemsAsync();

        public Task<Item> GetItemAsync(Guid id);
        Task CreateItemAsync(Item item);
        Task UpdateItemAsync(Item item);
        Task DeleteItemAsync(Guid id);
    }
}

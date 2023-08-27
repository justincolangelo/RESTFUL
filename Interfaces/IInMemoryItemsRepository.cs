using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RESTFUL.Entities;

namespace RESTFUL.Interfaces
{
    public interface IInMemoryItemsRepository
    {
        public IEnumerable<Item> GetItems();

        public Item GetItem(Guid id);
        void CreateItem(Item item);
        void UpdateItem(Item item);
        void DeleteItem(Guid id);
    }
}

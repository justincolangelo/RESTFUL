using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RESTFUL.Interfaces;
using RESTFUL.Entities;
using Microsoft.EntityFrameworkCore;
using RESTFUL.Context;

namespace RESTFUL.Repositories
{
    public class MSSQLItemsRepository : IItemsRepository
    {
        private readonly MSSQLContext _context;

        public MSSQLItemsRepository(MSSQLContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Item>> GetItemsAsync()
        {
            return await _context.Items.AsQueryable<Item>().ToListAsync();
        }

        public async Task<Item> GetItemAsync(Guid id)
        {
            return await _context.FindAsync<Item>(id);
        }

        public async Task CreateItemAsync(Item item)
        {
            await _context.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(Item item)
        {
            Item i = await _context.Items.FindAsync(item.Id);

            _context.Entry(i).CurrentValues.SetValues(item);
            await _context.SaveChangesAsync();

            await Task.CompletedTask;
            
        }

        public async Task DeleteItemAsync(Guid id)
        {
            Item i = await _context.Items.FindAsync(id);
            _context.Items.Remove(i);
            await _context.SaveChangesAsync();
        }
    }
}

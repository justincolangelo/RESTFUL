using Microsoft.EntityFrameworkCore;
using RESTFUL.Entities;

namespace RESTFUL.Context
{
    public class MSSQLContext : DbContext
    {
        public MSSQLContext(DbContextOptions options)
            : base(options) {
        
        }

        public DbSet<Item> Items { get; set; }
    }
}

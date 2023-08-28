using Microsoft.EntityFrameworkCore;
using RESTFUL.Entities;

namespace RESTFUL.Context
{
    public class PGSQLContext : DbContext
    {
        public PGSQLContext(DbContextOptions options)
            : base(options) {
        
        }

        public DbSet<Item> Items { get; set; }
    }
}

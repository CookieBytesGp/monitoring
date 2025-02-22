using Domain.Aggregates.Page.ValueObjects;
using Domain.Aggregates.Page;
using Domain.Aggregates.Tools;
using Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<Domain.Aggregates.Tools.Tool> Tools { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<BaseElement> BaseElements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure entity mappings and relationships
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }

    public DbSet<Domain.Aggregates.User.User> Users { get; set; }

    }

}

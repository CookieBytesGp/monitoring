using Domain.Aggregates.Tools;
using Microsoft.EntityFrameworkCore;
using Persistence.Page.Configurations;
using Persistence.Tools.Configuration;
using Domain.Aggregates.Page.ValueObjects;
using Persistence.User.Configurations;
using Domain.Aggregates.User;

namespace Persistence
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }


        public DbSet<Domain.Aggregates.User.User> Users { get; set; }
        public DbSet<Domain.Aggregates.Tools.Tool> Tools { get; set; }
        public DbSet<Domain.Aggregates.Page.Page> Pages { get; set; }

        // Ensure this line is commented out or removed
        // public DbSet<BaseElement> BaseElements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<BaseElement>(); // Ensure BaseElement is ignored as a non-owned entity

            //modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new PageConfiguration());
            modelBuilder.ApplyConfiguration(new ToolConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}

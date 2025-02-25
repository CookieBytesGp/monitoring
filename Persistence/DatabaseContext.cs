using Domain.Aggregates.Page.ValueObjects;
using Domain.Aggregates.Page;
using Domain.Aggregates.Tools;
using Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;
using Persistence.Tools;
using Persistence.BaseElemnt;
using Persistence.Page;

namespace Persistence
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<Domain.Aggregates.Tools.Tool> Tools { get; set; }
        public DbSet<Domain.Aggregates.Page.Page> Pages { get; set; }
        public DbSet<BaseElement> BaseElements { get; set; }
        public DbSet<Domain.Aggregates.User.User> Users { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    // Configure entity mappings and relationships
        //    modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
        //    base.OnModelCreating(modelBuilder);
        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
            modelBuilder.ApplyConfiguration(new Persistence.Page.Configuration.PageConfiguration());
            modelBuilder.ApplyConfiguration(new BaseElementConfiguration());
            modelBuilder.ApplyConfiguration(new Persistence.Tools.Configuration.ToolConfiguration());

            base.OnModelCreating(modelBuilder);
        }



    }
}

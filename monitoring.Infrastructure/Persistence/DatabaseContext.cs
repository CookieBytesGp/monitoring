using Microsoft.EntityFrameworkCore;
using Domain.Aggregates.Camera;
using Monitoring.Infrastructure.Configuration.Camera;
using Monitoring.Infrastructure.Configuration.Page;
using Monitoring.Infrastructure.Configuration.Tools;
using Domain.Aggregates.Page.ValueObjects;

namespace Monitoring.Infrastructure.Persistence;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }


    //public DbSet<Domain.Aggregates.User.User> Users { get; set; }
    public DbSet<Monitoring.Domain.Aggregates.Tools.Tool> Tools { get; set; }
    public DbSet<Monitoring.Domain.Aggregates.Page.Page> Pages { get; set; }
    public DbSet<Monitoring.Domain.Aggregates.Camera.Camera> cameras { get; set; }

    //// Ensure this line is commented out or removed
    //// public DbSet<BaseElement> BaseElements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<BaseElement>(); // Ensure BaseElement is ignored as a non-owned entity

        ////modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
        //modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new PageConfiguration());
        modelBuilder.ApplyConfiguration(new ToolConfiguration());

        // Camera configurations
        modelBuilder.ApplyConfiguration(new CameraEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CameraStreamEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CameraCapabilityEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CameraConfigurationEntityConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}

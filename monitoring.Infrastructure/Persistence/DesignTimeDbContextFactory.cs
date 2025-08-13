using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Monitoring.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        
        // Use a default connection string for design time
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MonitoringDb;Trusted_Connection=true;MultipleActiveResultSets=true");
        
        return new DatabaseContext(optionsBuilder.Options);
    }
}

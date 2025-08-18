using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Monitoring.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        
        // Use a default connection string for design time
        optionsBuilder.UseSqlServer("Server=.;Database=MonitoringDB;User Id=sa;Password=123456;TrustServerCertificate=True;");
        
        return new DatabaseContext(optionsBuilder.Options);
    }
}

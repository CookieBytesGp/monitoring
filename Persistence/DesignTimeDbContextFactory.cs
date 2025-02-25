using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
            public DatabaseContext CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

                // Use your desired connection string for migrations
                optionsBuilder.UseSqlite("Data Source = test.db");

                return new DatabaseContext(optionsBuilder.Options);
            }
    }
}

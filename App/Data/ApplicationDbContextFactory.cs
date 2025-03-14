using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace App.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=MonitoringSql;User ID=sa;Password=123456;Persist Security Info=True;TrustServerCertificate=True;");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }

}

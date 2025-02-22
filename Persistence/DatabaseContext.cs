using Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options : options)
        {
            
        }

    public DbSet<Domain.Aggregates.User.User> Users { get; set; }

    }

}

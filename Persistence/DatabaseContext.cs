using Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DataBaseContext : DbContext
    {

    }

    public DbSet<Domain.Aggregates.User.User> Users {  get; set; }
}

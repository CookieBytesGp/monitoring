using Application.Interfaces;

namespace Persistence.User
{
    public interface IUserRepository : IRepository<Domain.Aggregates.User.User>
    {
    }
}

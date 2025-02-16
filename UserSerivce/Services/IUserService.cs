using Domain.Aggregates.User;
using FluentResults;

namespace UserSerivce.Services
{
    public interface IUserService
    {
        Result<User> AddUser(string firstName, string lastName, string userName, string password);
        Result<User> GetUserByUserName(string userName);
        Result RemoveUser(string userName);
    }

}

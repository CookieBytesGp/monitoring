using Domain.Aggregates.User;
using FluentResults;

namespace UserSerivce.Services
{
    public class UserService : IUserService
    {
        private readonly List<User> _users = new List<User>();

        public Result<User> AddUser(string firstName, string lastName, string userName, string password)
        {
            var result = User.Create(firstName, lastName, userName, password);
            if (result.IsSuccess)
            {
                _users.Add(result.Value);
            }
            return result;
        }

        public Result<User> GetUserByUserName(string userName)
        {
            var user = _users.FirstOrDefault(u => u.UserName.Value == userName);
            if (user == null)
            {
                return Result.Fail<User>("User not found");
            }
            return Result.Ok(user);
        }

        public Result RemoveUser(string userName)
        {
            var user = _users.FirstOrDefault(u => u.UserName.Value == userName);
            if (user == null)
            {
                return Result.Fail("User not found");
            }
            _users.Remove(user);
            return Result.Ok();
        }

    }

}

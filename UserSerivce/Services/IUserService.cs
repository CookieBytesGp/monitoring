using Domain.Aggregates.User;
using Domain.Aggregates.User.ValueObjects;
using DTOs.User;
using FluentResults;

namespace UserSerivce.Services
{
    public interface IUserService
    {
        Task<Result<UserVeiwModel>> CreateUserAsync(FirstName firstName, LastName lastName, UserName userName, Password password);
        Task<UserVeiwModel> GetUserAsync(Guid userId); // متد نمونه برای دریافت اطلاعات کاربر
    }

}

using Domain.Aggregates.User;
using Domain.Aggregates.User.ValueObjects;
using DTOs.User;
using FluentResults;
using System;
using System.Threading.Tasks;
using UserSerivce.Services;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly Persistence.IUnitOfWork _unitOfWork;

        public UserService(Persistence.IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UserVeiwModel>> CreateUserAsync(FirstName firstName, LastName lastName, UserName userName, Password password)
        {
            var userResult = User.Create(firstName.Value, lastName.Value, userName.Value, password.Value);
            if (userResult.IsFailed)
            {
                return Result.Fail<UserVeiwModel>(userResult.Errors);
            }

            await _unitOfWork.UserRepository.AddAsync(entity: userResult.Value);
            await _unitOfWork.SaveAsync();

            var userViewModel = new UserVeiwModel
            {
                Id = userResult.Value.Id,
                FirstName = userResult.Value.FirstName,
                LastName = userResult.Value.LastName,
                UserName = userResult.Value.UserName,
                Password = userResult.Value.Password
            };

            return Result.Ok(userViewModel);
        }

        public async Task<UserVeiwModel> GetUserAsync(Guid userId)
        {
            var user = await GetUserFromDatabaseAsync(userId);
            if (user == null)
            {
                return null;
            }

            var userViewModel = new UserVeiwModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Password = user.Password
            };

            return userViewModel;
        }

        private async Task<User> GetUserFromDatabaseAsync(Guid userId)
        {
            // پیاده‌سازی عملیات جستجو در دیتابیس برای یافتن کاربر
            // اینجا باید با دیتابیس واقعی خود ارتباط برقرار کنید و کاربر مورد نظر را پیدا کنید
            return await Task.FromResult<User>(null);
        }
    }
}

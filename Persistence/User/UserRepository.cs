using CBG;
using Domain.Aggregates.User;
using Domain.Aggregates.User.ValueObjects;
using DTOs.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.User
{
    public class UserRepository : Repository<Domain.Aggregates.User.User>, IUserRepository
    {
        public UserRepository(DatabaseContext databaseContext) : base(databaseContext: databaseContext)
        {

        }
        public async Task<UserVeiwModel> GetByUserNameAsync(UserName username, CancellationToken cancellationToken)
        {

            var result =
                await
                DbSet
                .Where(current => current.UserName == username)
                .Select(current => new UserVeiwModel
                {
                    Id = current.Id,
                    FirstName = current.FirstName,
                    LastName = current.LastName,
                    UserName = current.UserName,
                    Password = current.Password
                })
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            return result;
        }
    }


}

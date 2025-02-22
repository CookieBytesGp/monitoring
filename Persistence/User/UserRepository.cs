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
    public class UserRepository(DataBaseContext databaseContext)
    {
    }

    //public async Task<UserVeiwModel> GetByUserNameAsync(UserName username, CancellationToken cancellationToken)
    //{
    //    var result =
    //        await
    //        DbSet
    //        .Where(current => current.Username == username)
    //        .Select(current => new UserVeiwModel
    //        {
    //            Id = current.Id,
    //            UserName = current.UserName,
    //            FisrtName = current.FisrtName,
    //            LastName = current.LastName,
    //            Password = current.Password
    //        })
    //        .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    //}
}

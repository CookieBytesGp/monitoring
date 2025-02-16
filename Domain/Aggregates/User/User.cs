using Domain.SeedWork;
using FluentResults;
using Domain.Aggregates.User.ValueObjects;

namespace Domain.Aggregates.User
{
    public class User : AggregateRoot
    {

        

        #region Ctor 

        private User() : base()
        {
            
        }

        private User(
            FirstName firstName,
            LastName lastName,
            UserName userName,
            Password password
            ) : this ()
        {
            FirstName = firstName;
            LastName = lastName;
            UserName = userName;
            Password = password;
        }

        #endregion

        #region Properties

        public FirstName FirstName { get; private set; }
        public LastName LastName { get; private set; }
        public UserName UserName { get; private set; }
        public Password Password { get; private set; }



        #endregion

        #region Methods

        public static Result<User> Create(
    string firstName,
    string lastName,
    string userName,
    string password)
        {
            var result = new Result<User>();

            var firstNameResult = FirstName.Create(firstName);
            if (firstNameResult.IsFailed)
            {
                result.WithErrors(firstNameResult.Errors);
            }

            var lastNameResult = LastName.Create(lastName);
            if (lastNameResult.IsFailed)
            {
                result.WithErrors(lastNameResult.Errors);
            }

            var userNameResult = UserName.Create(userName);
            if (userNameResult.IsFailed)
            {
                result.WithErrors(userNameResult.Errors);
            }

            var passwordResult = Password.Create(password);
            if (passwordResult.IsFailed)
            {
                result.WithErrors(passwordResult.Errors);
            }

  

            if (result.IsFailed)
            {
                return result;
            }

            var user = new User(
                firstNameResult.Value,
                lastNameResult.Value,
                userNameResult.Value,
                passwordResult.Value);

            result.WithValue(user);
            return result;
        }


        #endregion
    }
}

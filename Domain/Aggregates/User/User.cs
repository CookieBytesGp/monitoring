using Domain.SeedWork;
using FluentResults;
using UserManagment.API.Domain.Aggregates.Profile.ValueObjects;
using UserManagment.API.Domain.Aggregates.User.ValueObjects;

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
            Gender gender,
            NationalCode nationalCode,
            Role role
            ) : this ()
        {
            FirstName = firstName;
            LastName = lastName;
            Gender = gender;
            NationalCode = nationalCode;
            Role = role;
        }

        #endregion

        #region Properties

        public FirstName FirstName { get; private set; }
        public LastName LastName { get; private set; }
        public Gender Gender { get; private set; }
        public NationalCode NationalCode { get; private set; }
        public Role Role { get; private set; }



        #endregion

        #region Methods

        public static Result<User> Create(
    string firstName,
    string lastName,
    int gender,
    string nationalCode,
    int role)
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

            var genderResult = Gender.GetByValue(gender);
            if (genderResult.IsFailed)
            {
                result.WithErrors(genderResult.Errors);
            }

            var nationalCodeResult = NationalCode.Create(nationalCode);
            if (nationalCodeResult.IsFailed)
            {
                result.WithErrors(nationalCodeResult.Errors);
            }


            var roleResult = Role.GetByValue(role);
            if (roleResult.IsFailed)
            {
                result.WithErrors(roleResult.Errors);
            }

            if (result.IsFailed)
            {
                return result;
            }

            var user = new User(
                firstNameResult.Value,
                lastNameResult.Value,
                genderResult.Value,
                nationalCodeResult.Value,
                roleResult.Value);

            result.WithValue(user);
            return result;
        }


        #endregion
    }
}

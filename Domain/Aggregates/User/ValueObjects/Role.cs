using Domain.SeedWork;
using FluentResults;
using Resource;
using Resource.Messages;

namespace Domain.Aggregates.User.ValueObjects
{
    public class Role : Enumeration
    {
        #region Properties

        public static readonly Role User = new (0, DataDictionary.User);
        public static readonly Role Admin = new(0, DataDictionary.Admin);
        public static readonly Role SuperAdmin = new(0, DataDictionary.SuperAdmin);

        #endregion


        #region Constants

        public const int MaxLength = 50;

        #endregion


        #region Ctor

        private Role() { }

        private Role(int value, string name) : base(value, name)
        {
        }

        #endregion

        #region Methods

        public static Result<Role> GetByValue(int? value)
        {
            var result = new Result<Role>();

            if (value is null)
            {
                string errorMessage = string.Format(Validations.Required, DataDictionary.Role);

                result.WithError(errorMessage);

                return result;
            }

            var role = FromValue<Role>(value: value.Value);

            if (role is null)
            {
                string errorMessage = string.Format(Validations.InvalidCode, DataDictionary.Role);

                result.WithError(errorMessage);

                return result;
            }

            result.WithValue(role);

            return result;
        }


        #endregion
    }
}

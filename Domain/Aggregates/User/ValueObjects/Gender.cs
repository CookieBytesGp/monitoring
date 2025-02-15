using Domain.SeedWork;
using FluentResults;
using Resource;
using Resource.Messages;

namespace Domain.Aggregates.User.ValueObjects
{
    public class Gender : Enumeration
    {

        #region Properties

        public static readonly Gender Male = new (0, DataDictionary.Male);
        public static readonly Gender Female = new(0, DataDictionary.Female);

        #endregion

        #region Ctor

        private Gender() 
        {
            
        }

        private Gender(int value, string name) : base(value,name) { }

        #endregion

        #region Methods

        public static Result<Gender> GetByValue(int? value)
        {
            var result = new Result<Gender>();

            if (value is null)
            {
                string errorMessage = string.Format(Validations.Required, DataDictionary.Gender);

                result.WithError(errorMessage);

                return result;
            }

            var gender = FromValue<Gender>(value: value.Value);

            if (gender is null)
            {
                string errorMessage = string.Format(Validations.InvalidCode, DataDictionary.Gender);

                result.WithError(errorMessage);

                return result;
            }

            result.WithValue(gender);

            return result;
        }

        #endregion
    }
}

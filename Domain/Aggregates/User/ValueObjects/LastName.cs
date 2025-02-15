using FluentResults;
using UserManagment.API.Domain.SeedWork;
using Resource;
using Resource.Messages;

namespace Domain.Aggregates.User.ValueObjects
{
    public class LastName : ValueObject
    {

        #region Properties

        public string Value { get; set; }

        #endregion

        #region Constans

        public const int MaxLength = 50;

        #endregion

        #region Ctor

        private LastName() : base()
        {
            
        }

        private LastName(string value) : this()
        {

            Value = value;
            
        }

        #endregion

        #region Methods

        public static Result<LastName> Create(string value)
        {

            var result = new Result<LastName>();

            if(value == null)
            {
                string errorMessage = string.Format(Validations.Required, DataDictionary.LastName);

                result.WithError(errorMessage);

                return result;

            }

            if(value.Length > MaxLength )
            {
                string errorMessage = string.Format(Validations.MaxLength , DataDictionary.LastName);

                result.WithError(errorMessage);

                return result;

            }

            var returnValue = new LastName(value);

            result.WithValue(returnValue);

            return returnValue;


        }

        #endregion

    }
}

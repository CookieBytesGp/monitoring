using Domain.SeedWork;
using FluentResults;
using Resource.Messages;
using Resource;

namespace Domain.Aggregates.User.ValueObjects
{
    public class FirstName : ValueObject
    {

        #region Properties

        public string Value { get; set; }

        #endregion

        #region Constans

        public const int MaxLength = 50;

        #endregion

        #region Ctor

        private FirstName() : base()
        {

        }


        private FirstName(string value) : this()
        {

            Value = value;

        }

        #endregion

        #region Methods

        public static Result<FirstName> Create(string value)
        {

            var result = new Result<FirstName>();

            if (value == null)
            {
                string errorMessage = string.Format(Validations.Required, DataDictionary.FirstName);

                result.WithError(errorMessage);

                return result;

            }

            if (value.Length > MaxLength)
            {
                string errorMessage = string.Format(Validations.MaxLength, DataDictionary.FirstName);

                result.WithError(errorMessage);

                return result;

            }

            var returnValue = new FirstName(value);

            result.WithValue(returnValue);

            return result;


        }

        protected override System.Collections.Generic.IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }


        #endregion


    }
}

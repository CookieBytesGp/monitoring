using FluentResults;
using Resource.Messages;
using Resource;
using Domain.SeedWork;

namespace Domain.Aggregates.User.ValueObjects
{
    public class Password : ValueObject
    {
        #region Properties

        public string Value { get; private set; }

        #endregion

        #region Constans

        public const int MaxLength = 100;
        public const int MinLength = 8;

        #endregion

        #region Ctor

        private Password() : base() { }

        private Password(string value) : this() { Value = value; }

        #endregion

        #region Methods

        public static Result<Password> Create(string value)
        {
            var result = new Result<Password>();

            if (string.IsNullOrEmpty(value))
            {
                string errorMessage = string.Format(Validations.Required, DataDictionary.Password);
                result.WithError(errorMessage);
                return result;
            }

            if (value.Length > MaxLength)
            {
                string errorMessage = string.Format(Validations.MaxLength, DataDictionary.Password);
                result.WithError(errorMessage);
                return result;
            }

            if (value.Length < MinLength)
            {
                string errorMessage = string.Format(Validations.MinLength, DataDictionary.Password);
                result.WithError(errorMessage);
                return result;
            }

            var password = new Password(value);
            result.WithValue(password);
            return result;
        }

        protected override System.Collections.Generic.IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }


        #endregion
    }
}

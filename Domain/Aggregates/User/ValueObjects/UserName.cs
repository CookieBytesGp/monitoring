using FluentResults;
using Resource.Messages;
using Resource;
using Domain.SeedWork;
using System.Text.Json.Serialization;

namespace Domain.Aggregates.User.ValueObjects
{
    public class UserName :ValueObject
    {
        #region Properties

        public string Value { get; }

        #endregion

        #region Constans

        public const int MaxLength = 20;
        public const int MinLength = 3;

        #endregion

        #region Ctor

        private UserName() : base() 
        {
        }
        [JsonConstructor]
        private UserName(string value) : this()
        {
            Value = value;
        }

        #endregion

        #region Methods

        public static Result<UserName> Create(string value)
        {
            var result = new Result<UserName>();

            if (string.IsNullOrEmpty(value))
            {
                string errorMessage = string.Format(Validations.Required, DataDictionary.Username);
                result.WithError(errorMessage);
                return result;
            }

            if (value.Length > MaxLength)
            {
                string errorMessage = string.Format(Validations.MaxLength, DataDictionary.Username);
                result.WithError(errorMessage);
                return result;
            }

            if (value.Length < MinLength)
            {
                string errorMessage = string.Format(Validations.MinLength, DataDictionary.Username);
                result.WithError(errorMessage);
                return result;
            }

            var returnValue = new UserName(value);
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

using FluentResults;
using Resource;
using Resource.Messages;
using System.Text.RegularExpressions;
using UserManagment.API.Domain.SeedWork;
using UserManagment.API.Framwork.UserManagment;

namespace Domain.Aggregates.User.ValueObjects
{
    public class NationalCode : ValueObject
    {

        #region Properties

        public string Value { get; set; }

        #endregion

        #region Constans

        public const int FixLength = 10;

        public const string ReqularExpression = "^[0-9]{10}$";

        #endregion

        #region Ctor

        private NationalCode() : base()
        {
            
        }

        private NationalCode(string value) : this()
        {

            Value = value;
            
        }

        #endregion

        #region Methods

        public static Result<NationalCode> Create(string value)
        {
            var result = new Result<NationalCode>();

            value = value.Fix();

            if(value is null)
            {
                string errorMessage = string.Format(Validations.Required, DataDictionary.NationalCode);

                result.WithError(errorMessage);

                return result;
            
            }

            if(value.Length != FixLength)
            {
                string errorMessage = string.Format(Validations.Length, DataDictionary.NationalCode);

                result.WithError(errorMessage);

                return result;
            }

            if(Regex.IsMatch(value, ReqularExpression) == false)
            {
                string errorMessage = string.Format(Validations.NotLegal, DataDictionary.NationalCode);

                result.WithError(errorMessage);

                return result;

            }

            var returnValue = new NationalCode(value);

            result.WithValue(returnValue);

            return result;
        }

        #endregion
    }
}

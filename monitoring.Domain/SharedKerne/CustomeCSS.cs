using Monitoring.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.SharedKernel
{
    public class CustomCss : ValueObject
    {
        #region Properties

        private Dictionary<string, string> properties;

        #endregion

        #region Ctor

        public CustomCss()
        {
            properties = new Dictionary<string, string>();
        }

        #endregion

        #region Methods

        public void SetProperty(string property, string value)
        {
            properties[property] = value;
        }

        public string GetProperty(string property)
        {
            return properties.TryGetValue(property, out var value) ? value : null;
        }

        public string ToCssString()
        {
            var cssBuilder = new StringBuilder();
            foreach (var property in properties)
            {
                cssBuilder.AppendLine($"{property.Key}: {property.Value};");
            }
            return cssBuilder.ToString();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            foreach (var property in properties)
            {
                yield return property.Key;
                yield return property.Value;
            }
        }

        #endregion
    }

}

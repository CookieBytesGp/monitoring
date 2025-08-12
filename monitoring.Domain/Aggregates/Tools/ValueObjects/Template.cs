using Monitoring.Domain.SeedWork;
using FluentResults;
using System;
using System.Collections.Generic;

namespace Domain.Aggregates.Tools.ValueObjects
{
    public class Template : ValueObject
    {
        #region Properties

        public string HtmlStructure { get; private set; }
        public Dictionary<string, string> DefaultCssClasses { get; private set; }
        public string DefaultCss { get; private set; }

        #endregion

        #region Constructor

        private Template() { }

        private Template(string htmlStructure, Dictionary<string, string> defaultCssClasses, string defaultCss)
        {
            HtmlStructure = htmlStructure;
            DefaultCssClasses = defaultCssClasses ?? new Dictionary<string, string>();
            DefaultCss = defaultCss;
        }

        #endregion

        #region Methods

        public static Result<Template> Create(string htmlStructure, Dictionary<string, string> defaultCssClasses, string defaultCss)
        {
            var result = new Result<Template>();

            if (string.IsNullOrWhiteSpace(htmlStructure))
            {
                result.WithError("HTML structure is required.");
            }

            if (defaultCssClasses == null || defaultCssClasses.Count == 0)
            {
                result.WithError("Default CSS classes are required.");
            }

            if (string.IsNullOrWhiteSpace(defaultCss))
            {
                result.WithError("Default CSS is required.");
            }

            if (result.IsFailed)
            {
                return result;
            }

            var template = new Template(htmlStructure, defaultCssClasses, defaultCss);
            result.WithValue(template);

            return result;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HtmlStructure;
            foreach (var cssClass in DefaultCssClasses)
            {
                yield return cssClass.Key;
                yield return cssClass.Value;
            }
            yield return DefaultCss;
        }

        #endregion
    }
}

using Domain.SeedWork;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Page.ValueObjects
{
    public class TemplateBody : ValueObject
    {
        #region Properties

        public string HtmlTemplate { get; private set; }
        public Dictionary<string, string> DefaultCssClasses { get; private set; }
        public string CustomCss { get; private set; }
        public string CustomJs { get; private set; }
        public bool IsFloating { get; private set; }

        #endregion

        #region Constructor

        private TemplateBody()
        {
        }

        private TemplateBody(
            string htmlTemplate,
            Dictionary<string, string> defaultCssClasses,
            string customCss,
            string customJs,
            bool isFloating)
        {
            HtmlTemplate = htmlTemplate;
            DefaultCssClasses = defaultCssClasses;
            CustomCss = customCss ?? string.Empty;
            CustomJs = customJs ?? string.Empty;
            IsFloating = isFloating;
        }

        #endregion

        #region Methods

        public static Result<TemplateBody> Create(
            string htmlTemplate,
            Dictionary<string, string> defaultCssClasses,
            string customCss = null,
            string customJs = null,
            bool isFloating = false)
        {
            var result = new Result<TemplateBody>();

            if (string.IsNullOrWhiteSpace(htmlTemplate))
            {
                result.WithError("HTML Template is required.");
                return result;
            }

            if (defaultCssClasses == null || defaultCssClasses.Count == 0)
            {
                result.WithError("Default CSS classes are required.");
                return result;
            }

            var templateBody = new TemplateBody(htmlTemplate, defaultCssClasses, customCss, customJs, isFloating);
            result.WithValue(templateBody);
            return result;
        }

        public void UpdateCustomCss(string customCss)
        {
            CustomCss = customCss ?? string.Empty;
        }

        public void UpdateCustomJs(string customJs)
        {
            CustomJs = customJs ?? string.Empty;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HtmlTemplate;
            foreach (var cssClass in DefaultCssClasses)
            {
                yield return cssClass.Key;
                yield return cssClass.Value;
            }
            yield return CustomCss;
            yield return CustomJs;
            yield return IsFloating;
        }

        #endregion
    }

}

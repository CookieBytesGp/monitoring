using Domain.SeedWork;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.SharedKernel
{
    public class Asset : ValueObject
    {
        #region Properties

        public string Url { get; private set; }
        public string Type { get; private set; }
        public string AltText { get; private set; }
        public string Content { get; private set; } // Element content as a string
        public Dictionary<string, string> Metadata { get; private set; }

        #endregion

        #region Constructor

        private Asset()
        {
            // Required by EF Core
        }

        private Asset(
            string url,
            string type,
            string content,
            string altText = null,
            Dictionary<string, string> metadata = null)
        {
            Url = url;
            Type = type;
            Content = content;
            AltText = altText;
            Metadata = metadata ?? new Dictionary<string, string>();
        }

        #endregion

        #region Methods

        public static Result<Asset> Create(
            string url,
            string type,
            string content,
            string altText = null,
            Dictionary<string, string> metadata = null)
        {
            var result = new Result<Asset>();

            // Validate URL or Content based on Type
            if (type == "text" && string.IsNullOrWhiteSpace(content))
            {
                result.WithError("Content is required for text assets.");
            }
            else if (type != "text" && string.IsNullOrWhiteSpace(url))
            {
                result.WithError("URL is required for non-text assets.");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                result.WithError("Type is required.");
            }

            if (result.IsFailed)
            {
                return result;
            }

            var asset = new Asset(
                url: url,
                type: type,
                content: content,
                altText: altText,
                metadata: metadata);

            result.WithValue(asset);

            return result;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Url;
            yield return Type;
            yield return Content;
            yield return AltText;
            foreach (var meta in Metadata)
            {
                yield return meta.Key;
                yield return meta.Value;
            }
        }

        #endregion
    }


}

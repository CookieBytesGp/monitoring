using Monitoring.Domain.SeedWork;
using FluentResults;
using System;

namespace Domain.Aggregates.Page.ValueObjects
{
    /// <summary>
    /// تنظیمات نمایش صفحه - Value Object
    /// </summary>
    public class DisplayConfiguration : ValueObject
    {
        #region Properties

        public int Width { get; private set; }
        public int Height { get; private set; }
        public string ThumbnailUrl { get; private set; }
        public DisplayOrientation Orientation { get; private set; }

        // Computed Properties
        public double AspectRatio => (double)Width / Height;

        #endregion

        #region Constructor

        private DisplayConfiguration()
        {
            // Required by EF Core
        }

        private DisplayConfiguration(int width, int height, DisplayOrientation orientation, string thumbnailUrl = null)
        {
            Width = width;
            Height = height;
            Orientation = orientation;
            ThumbnailUrl = thumbnailUrl;
        }

        #endregion

        #region Methods

        public static Result<DisplayConfiguration> Create(int width, int height, DisplayOrientation orientation, string thumbnailUrl = null)
        {
            var result = new Result<DisplayConfiguration>();

            if (width <= 0)
            {
                result.WithError("Width must be greater than zero.");
            }

            if (height <= 0)
            {
                result.WithError("Height must be greater than zero.");
            }

            if (orientation == null)
            {
                result.WithError("Display orientation is required.");
            }

            if (width > 7680 || height > 4320) // 8K resolution limit
            {
                result.WithError("Display dimensions exceed maximum allowed resolution (8K).");
            }

            if (!string.IsNullOrWhiteSpace(thumbnailUrl) && thumbnailUrl.Length > 500)
            {
                result.WithError("Thumbnail URL is too long.");
            }

            if (result.IsFailed)
            {
                return result;
            }

            var displayConfig = new DisplayConfiguration(width, height, orientation, thumbnailUrl);
            result.WithValue(displayConfig);
            return result;
        }

        public DisplayConfiguration UpdateThumbnail(string thumbnailUrl)
        {
            return new DisplayConfiguration(Width, Height, Orientation, thumbnailUrl);
        }

        public bool IsWidescreen()
        {
            return AspectRatio >= 1.6;
        }

        public bool IsUltraWide()
        {
            return AspectRatio >= 2.1;
        }

        public string GetCommonAspectRatio()
        {
            return Math.Round(AspectRatio, 2) switch
            {
                >= 1.77 and <= 1.78 => "16:9",
                >= 1.33 and <= 1.34 => "4:3",
                >= 0.99 and <= 1.01 => "1:1",
                >= 0.56 and <= 0.57 => "9:16",
                >= 2.33 and <= 2.34 => "21:9",
                _ => $"{Width}:{Height}"
            };
        }

        #endregion

        #region Value Object Implementation

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Width;
            yield return Height;
            yield return Orientation;
            yield return ThumbnailUrl ?? string.Empty;
        }

        #endregion
    }
}

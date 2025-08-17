using Domain.Aggregates.Page.ValueObjects;
using Monitoring.Domain.SeedWork;
using FluentResults;
using Domain.SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitoring.Domain.Aggregates.Page
{
    public class Page : AggregateRoot
    {
        #region Properties

        public string Title { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        
        // فیلدهای جدید
        public PageStatus Status { get; private set; }
        public DisplayConfiguration DisplayConfig { get; private set; }
        
        public Asset BackgroundAsset { get; private set; }

        private readonly List<BaseElement> _elements;
        public IReadOnlyCollection<BaseElement> Elements => _elements.AsReadOnly();

        #endregion

        #region Constructor

        private Page()
        {
            _elements = new List<BaseElement>();
        }

        private Page(string title, DisplayConfiguration displayConfig)
        {
            Title = title;
            DisplayConfig = displayConfig;
            Status = PageStatus.Draft;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            _elements = new List<BaseElement>();
        }

        #endregion

        #region Methods

        public static Result<Page> Create(string title, int displayWidth, int displayHeight, DisplayOrientation orientation)
        {
            var result = new Result<Page>();

            if (string.IsNullOrWhiteSpace(title))
            {
                result.WithError("Title is required.");
            }

            var displayConfigResult = DisplayConfiguration.Create(displayWidth, displayHeight, orientation);
            if (displayConfigResult.IsFailed)
            {
                result.WithErrors(displayConfigResult.Errors);
            }

            if (result.IsFailed)
            {
                return result;
            }

            var page = new Page(title, displayConfigResult.Value);

            result.WithValue(page);
            return result;
        }

        public void AddElement(BaseElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            _elements.Add(element);
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveElement(BaseElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            _elements.Remove(element);
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveElementById(Guid elementId)
        {
            var element = _elements.FirstOrDefault(e => e.Id == elementId);
            if (element != null)
            {
                _elements.Remove(element);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void UpdateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.", nameof(title));

            Title = title;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string title, List<BaseElement> elements)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title cannot be empty.", nameof(title));
            }

            Title = title;
            UpdatedAt = DateTime.UtcNow;

            if (elements != null && elements.Count > 0)
            {
                _elements.Clear();
                _elements.AddRange(elements);
            }
        }

        public void SetBackgroundAsset(Asset asset)
        {
            if (asset != null && !asset.IsAudio() && !asset.IsVideo())
            {
                throw new ArgumentException("Background asset must be audio or video.");
            }
            
            BackgroundAsset = asset;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveBackgroundAsset()
        {
            BackgroundAsset = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDisplayConfiguration(DisplayConfiguration displayConfig)
        {
            DisplayConfig = displayConfig ?? throw new ArgumentNullException(nameof(displayConfig));
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDisplaySize(int width, int height, DisplayOrientation orientation)
        {
            var displayConfigResult = DisplayConfiguration.Create(width, height, orientation, DisplayConfig?.ThumbnailUrl);
            if (displayConfigResult.IsFailed)
                throw new ArgumentException(string.Join(", ", displayConfigResult.Errors));
                
            DisplayConfig = displayConfigResult.Value;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDisplaySize(int width, int height)
        {
            if (DisplayConfig == null)
                throw new InvalidOperationException("Display configuration is not set.");
                
            UpdateDisplaySize(width, height, DisplayConfig.Orientation);
        }

        public void SetStatus(PageStatus status)
        {
            Status = status ?? throw new ArgumentNullException(nameof(status));
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetThumbnail(string thumbnailUrl)
        {
            if (DisplayConfig == null)
                throw new InvalidOperationException("Display configuration is not set.");
                
            DisplayConfig = DisplayConfig.UpdateThumbnail(thumbnailUrl);
            UpdatedAt = DateTime.UtcNow;
        }

        public BaseElement GetElementById(Guid id)
        {
            return _elements.FirstOrDefault(e => e.Id == id);
        }

        public void ReorderElements(List<(Guid elementId, int newOrder)> orderChanges)
        {
            foreach (var (elementId, newOrder) in orderChanges)
            {
                var element = _elements.FirstOrDefault(e => e.Id == elementId);
                element?.UpdateOrder(newOrder);
            }
            
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }

}

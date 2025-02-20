using Domain.Aggregates.Page.ValueObjects;
using Domain.SeedWork;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Page
{
    public class Page : AggregateRoot
    {
        #region Properties

        public string Title { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<BaseElement> _elements;
        public IReadOnlyCollection<BaseElement> Elements => _elements.AsReadOnly();

        #endregion

        #region Constructor

        private Page()
        {
            _elements = new List<BaseElement>();
        }

        private Page(string title)
        {
            Title = title;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            _elements = new List<BaseElement>();
        }

        #endregion

        #region Methods

        public static Result<Page> Create(string title)
        {
            var result = new Result<Page>();

            if (string.IsNullOrWhiteSpace(title))
            {
                result.WithError("Title is required.");
                return result;
            }

            var page = new Page(title);

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

        public void UpdateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.", nameof(title));

            Title = title;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }

}

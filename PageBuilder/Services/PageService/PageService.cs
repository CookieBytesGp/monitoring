using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using Domain.Aggregates.Page;
using Domain.Aggregates.Page.ValueObjects;
using Persistence.Page;

namespace PageBuilder.Services.PageService
{
    public class PageService : IPageService
    {
        private readonly IPageRepository _pageRepository;

        public PageService(IPageRepository pageRepository)
        {
            _pageRepository = pageRepository;
        }

        public async Task<Result<Page>> GetByIdAsync(Guid id)
        {
            var page = await _pageRepository.GetByIdAsync(id);
            if (page == null)
            {
                return Result.Fail<Page>("Page not found.");
            }
            return Result.Ok(page);
        }

        public async Task<Result<IEnumerable<Page>>> GetAllAsync()
        {
            var pages = await _pageRepository.GetAllAsync();
            return Result.Ok(pages);
        }

        public async Task<Result<Page>> CreateAsync(string title) // Update return type to Result<Page>
        {
            var pageResult = Page.Create(title);

            if (pageResult.IsFailed)
            {
                return Result.Fail(pageResult.Errors);
            }

            await _pageRepository.AddAsync(pageResult.Value);
            return Result.Ok(pageResult.Value); // Ensure to return the created Page
        }

        public async Task<Result> UpdateAsync(Guid id, string title)
        {
            var existingPage = await _pageRepository.GetByIdAsync(id);

            if (existingPage == null)
            {
                return Result.Fail("Page not found.");
            }

            existingPage.UpdateTitle(title);
            await _pageRepository.UpdateAsync(existingPage);

            return Result.Ok();
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var page = await _pageRepository.GetByIdAsync(id);

            if (page == null)
            {
                return Result.Fail("Page not found.");
            }

            await _pageRepository.DeleteAsync(page.Id);
            return Result.Ok();
        }

        public async Task<Result> AddElementAsync(Guid pageId, BaseElement element)
        {
            var page = await _pageRepository.GetByIdAsync(pageId);

            if (page == null)
            {
                return Result.Fail("Page not found.");
            }

            page.AddElement(element);
            await _pageRepository.UpdateAsync(page);

            return Result.Ok();
        }

        public async Task<Result> RemoveElementAsync(Guid pageId, BaseElement element)
        {
            var page = await _pageRepository.GetByIdAsync(pageId);

            if (page == null)
            {
                return Result.Fail("Page not found.");
            }

            page.RemoveElement(element);
            await _pageRepository.UpdateAsync(page);

            return Result.Ok();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Aggregates.Page.ValueObjects;
using FluentResults;

namespace PageBuilder.Services.PageService
{
    public interface IPageService
    {
        Task<Result<Domain.Aggregates.Page.Page>> GetByIdAsync(Guid id);
        Task<Result<IEnumerable<Domain.Aggregates.Page.Page>>> GetAllAsync();
        Task<Result<Domain.Aggregates.Page.Page>> CreateAsync(string title);
        Task<Result> UpdateAsync(Guid id, string title);
        Task<Result> DeleteAsync(Guid id);
        Task<Result> AddElementAsync(Guid pageId, BaseElement element);
        Task<Result> RemoveElementAsync(Guid pageId, BaseElement element);

    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using DTOs.Pagebuilder;

namespace PageBuilder.Services.PageService
{
    public interface IPageService
    {
        Task<Result<PageDTO>> CreatePageAsync(string title, List<BaseElementDTO> elements);
        Task<PageDTO> GetPageAsync(Guid id);
        Task<Result<IEnumerable<PageDTO>>> GetAllAsync();
        Task<Result> UpdatePageAsync(Guid id, string title, List<BaseElementDTO> elements);
        Task<Result> DeletePageAsync(Guid id);

        // Element operations
        Task<Result> AddElementAsync(Guid pageId, BaseElementDTO elementDTO);
        Task<Result> RemoveElementAsync(Guid pageId, Guid elementId);
        Task<Result> UpdateElementAsync(Guid pageId, Guid elementId, BaseElementDTO elementDTO);
    }
}


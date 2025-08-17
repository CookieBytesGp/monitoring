using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using DTOs.Pagebuilder;
using Domain.SharedKernel;
using Domain.Aggregates.Page.ValueObjects;
using Monitoring.Application.DTOs.Page;

namespace Monitoring.Application.Services.Page
{
    public interface IPageService
    {
        // Core Page operations
        Task<Result<PageDTO>> CreatePageAsync(string title, int displayWidth, int displayHeight, DisplayOrientation orientation, List<BaseElementDTO> elements = null);
        Task<Result<PageDTO>> GetPageAsync(Guid id);
        Task<Result<IEnumerable<PageDTO>>> GetAllAsync();
        Task<Result> UpdatePageAsync(Guid id, string title, List<BaseElementDTO> elements);
        Task<Result> DeletePageAsync(Guid id);

        // Element operations (handled separately)
        Task<Result> AddElementAsync(Guid pageId, BaseElementDTO elementDTO);
        Task<Result> RemoveElementAsync(Guid pageId, Guid elementId);
        Task<Result> UpdateElementAsync(Guid pageId, Guid elementId, BaseElementDTO elementDTO);
        Task<Result> ReorderElementsAsync(Guid pageId, List<(Guid elementId, int newOrder)> orderChanges);

        // Page-specific operations
        Task<Result> SetPageStatusAsync(Guid pageId, PageStatus status);
        Task<Result> SetPageThumbnailAsync(Guid pageId, string thumbnailUrl);
        Task<Result> SetPageDisplaySizeAsync(Guid pageId, int width, int height, DisplayOrientation orientation);
        Task<Result> SetPageDisplaySizeAsync(Guid pageId, int width, int height);
        Task<Result> SetBackgroundAssetAsync(Guid pageId, Asset asset);
        Task<Result> RemoveBackgroundAssetAsync(Guid pageId);
    }
}


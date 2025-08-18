using Monitoring.Ui.Models.Page;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monitoring.Ui.Interfaces
{
    public interface IPageApiService
    {
        Task<List<PageViewModel>> GetAllPagesAsync();
        Task<PageViewModel> GetPageByIdAsync(Guid id);
        Task<PageViewModel> CreatePageAsync(CreatePageRequest request);
        Task<bool> UpdatePageAsync(Guid id, UpdatePageRequest request);
        Task<bool> UpdateDisplaySizeAsync(Guid id, UpdateDisplaySizeRequest request);
        Task<bool> UpdateStatusAsync(Guid id, UpdateStatusRequest request);
        Task<bool> UpdateThumbnailAsync(Guid id, UpdateThumbnailRequest request);
        Task<bool> DeletePageAsync(Guid id);
    }
}

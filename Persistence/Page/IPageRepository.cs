using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Persistence.Page
{
    public interface IPageRepository
    {
        Task<Domain.Aggregates.Page.Page> GetByIdAsync(Guid id);
        Task<IEnumerable<Domain.Aggregates.Page.Page>> GetAllAsync();
        Task AddAsync(Domain.Aggregates.Page.Page page);
        Task UpdateAsync(Domain.Aggregates.Page.Page page);
        Task DeleteAsync(Guid id);
    }
}

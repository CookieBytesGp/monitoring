using Domain.Aggregates.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Tools
{
    public interface IToolRepository
    {
        Task<Tool> GetByIdAsync(Guid id);
        Task<IEnumerable<Tool>> GetAllAsync();
        Task AddAsync(Tool tool);
        Task UpdateAsync(Tool tool);
        Task DeleteAsync(Guid id);
    }
}

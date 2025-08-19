using Monitoring.Domain.Aggregates.Tools;
using Monitoring.Common.Interfaces;

namespace Monitoring.Infrastructure.Repositories.Tools
{
    public interface IToolRepository : IRepository<Tool>
    {
        Task<List<Tool>> GetByElementTypeAsync(string elementType);
    }
}

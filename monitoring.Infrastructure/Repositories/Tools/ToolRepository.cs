using Monitoring.Domain.Aggregates.Tools;
using Microsoft.EntityFrameworkCore;
using Monitoring.Common.Repositories;
using Monitoring.Infrastructure.Persistence;
using Monitoring.Infrastructure.Repositories.Tools;

namespace Monitoring.Infrastructure.Repositories.Tools
{
    public class ToolRepository : Repository<Tool>, IToolRepository
    {
        public ToolRepository(DatabaseContext databaseContext) : base(databaseContext: databaseContext)
        {
        }

        public async Task<List<Tool>> GetByElementTypeAsync(string elementType)
        {
            return await DbSet
                .Where(t => t.ElementType == elementType)
                .ToListAsync();
        }
    }
}

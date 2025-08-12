using Domain.Aggregates.Page;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Monitoring.Common.Repositories;
using Monitoring.Infrastructure.Persistence;

namespace Monitoring.Infrastructure.Repositories.Page
{
    public class PageRepository : Repository<Domain.Aggregates.Page.Page>, IPageRepository
    {
        public PageRepository(DatabaseContext databaseContext) : base(databaseContext)
        {
        }

        public async Task<Domain.Aggregates.Page.Page> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(p => p.Elements)
                .FirstOrDefaultAsync(p => p.Title == title, cancellationToken);
        }
    }
}

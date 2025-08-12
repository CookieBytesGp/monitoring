using Domain.Aggregates.Page;
using Monitoring.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Monitoring.Infrastructure.Repositories.Page
{
    public interface IPageRepository : IRepository<Monitoring.Domain.Aggregates.Page.Page>
    {
        Task<Domain.Aggregates.Page.Page> GetByTitleAsync(string title, CancellationToken cancellationToken = default);
    }
}

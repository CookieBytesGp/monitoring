using Domain.Aggregates.Page;
using Application.Interfaces;

namespace Persistence.Page
{
    public interface IPageRepository : IRepository<Domain.Aggregates.Page.Page>
    {
        // Add any additional methods specific to PageRepository if needed
    }
}

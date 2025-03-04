using Domain.Aggregates.Tools;
using Application.Interfaces;

namespace Persistence.Tool
{
    public interface IToolRepository : IRepository<Domain.Aggregates.Tools.Tool>
    {
        // Add any additional methods specific to ToolRepository if needed
    }
}

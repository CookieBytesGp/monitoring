using Domain.Aggregates.Tools;
using Monitoring.Common.Interfaces;

namespace Persistence.Tool
{
    public interface IToolRepository : IRepository<Monitoring.Domain.Aggregates.Tools.Tool>
    {
        // Add any additional methods specific to ToolRepository if needed
    }
}

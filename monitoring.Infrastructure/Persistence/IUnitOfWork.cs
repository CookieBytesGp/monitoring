
using Monitoring.Infrastructure.Repositories.Page;
using Persistence.Camera;

namespace Monitoring.Infrastructure.Persistence;

public interface IUnitOfWork : Monitoring.Common.Utilities.IUnitOfWork
{

    ICameraRepository CameraRepository { get; }
    IPageRepository PageRepository { get; }

    /// <summary>
    /// Clears the EF Core change tracker to resolve concurrency conflicts
    /// </summary>
    void ClearChangeTracker();

    /// <summary>
    /// Gets database connection for direct SQL operations (used for critical operations like force abandon)
    /// </summary>
    System.Data.Common.DbConnection GetDbConnection();
}

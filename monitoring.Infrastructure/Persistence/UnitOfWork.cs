
using Microsoft.EntityFrameworkCore;
using Monitoring.Infrastructure.Persistence;
using Monitoring.Infrastructure.Repositories.Page;
using Persistence.Camera;

namespace Shahzade.Infrastructure.Persistence;

/// <summary>
/// پیاده‌سازی UnitOfWork برای مدیریت دسترسی به Repository‌ها
/// </summary>
public class UnitOfWork : Monitoring.Common.Utilities.UnitOfWork<DatabaseContext>, IUnitOfWork
{

    public UnitOfWork(DatabaseContext databaseContext) : base(databaseContext)
    {
    }

    #region Camera

    private ICameraRepository _cameraRepository;

    public ICameraRepository CameraRepository
    {
        get
        {
            if (_cameraRepository == null)
            {
                _cameraRepository = new CameraRepository(databaseContext: DatabaseContext);
            }

            return _cameraRepository;
        }
    }

    #endregion

    #region Page

    private IPageRepository _pageRepository;

    public IPageRepository PageRepository
    {
        get
        {
            if (_pageRepository == null)
            {
                _pageRepository = new PageRepository(databaseContext: DatabaseContext);
            }

            return _pageRepository;
        }
    }


#endregion



    #region Change Tracking Management

    /// <summary>
    /// Clears the EF Core change tracker to resolve concurrency conflicts
    /// </summary>
    public void ClearChangeTracker()
    {
        DatabaseContext.ChangeTracker.Clear();
    }

    /// <summary>
    /// Gets database connection for direct SQL operations (used for critical operations like force abandon)
    /// </summary>
    public System.Data.Common.DbConnection GetDbConnection()
    {
        return DatabaseContext.Database.GetDbConnection();
    }

    #endregion
}
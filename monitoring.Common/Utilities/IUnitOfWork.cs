
using System;
using System.Threading.Tasks;
using FluentResults;

namespace Monitoring.Common.Utilities;

public interface IUnitOfWork : IDisposable
{
    bool IsDisposed { get; }

    Task<int> SaveAsync();
    void BeginTransaction();
    void Commit();
    void Rollback();

    /// <summary>
    /// Save changes with concurrency handling and retry mechanism
    /// </summary>
    /// <param name="retryAction">Optional retry action to refresh data</param>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> SaveChangesWithConcurrencyHandlingAsync(Func<Task> retryAction = null, int maxRetries = 3);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FluentResults;

namespace Monitoring.Common.Utilities;

public abstract class UnitOfWork<TDbContext> :
    IUnitOfWork where TDbContext : DbContext
{
    private IDbContextTransaction _transaction;

    public UnitOfWork(TDbContext databaseContext) : base()
    {
        DatabaseContext = databaseContext;
    }

    // **********
    protected TDbContext DatabaseContext { get; }
    // **********

    // **********
    /// <summary>
    /// To detect redundant calls
    /// </summary>
    public bool IsDisposed { get; protected set; }
    // **********

    /// <summary>
    /// Public implementation of Dispose pattern callable by consumers.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            // TODO: dispose managed state (managed objects).

            if (DatabaseContext != null)
            {
                _transaction?.Dispose();
                DatabaseContext.Dispose();
            }
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        IsDisposed = true;
    }

    public async Task<int> SaveAsync()
    {
        try
        {
            int result = await DatabaseContext.SaveChangesAsync();
            return result;
        }
        catch (Exception ex)
        {
            // Handle the exception as needed
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }



    // ✅ Added transaction support
    public void BeginTransaction() => _transaction = DatabaseContext.Database.BeginTransaction();

    public void Commit()
    {
        _transaction?.Commit();
        _transaction?.Dispose();
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
    }

    /// <summary>
    /// Save changes with concurrency handling and retry mechanism
    /// </summary>
    /// <param name="retryAction">Optional retry action to refresh data</param>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result> SaveChangesWithConcurrencyHandlingAsync(Func<Task> retryAction = null, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await DatabaseContext.SaveChangesAsync();
                return Result.Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (i == maxRetries - 1)
                    return Result.Fail("داده‌ها توسط کاربر دیگری تغییر کرده‌اند. لطفاً صفحه را به‌روزرسانی کرده و دوباره تلاش کنید.");

                // Execute retry action if provided
                if (retryAction != null)
                {
                    try
                    {
                        await retryAction();
                    }
                    catch (Exception retryEx)
                    {
                        return Result.Fail($"خطا در تلاش مجدد: {retryEx.Message}");
                    }
                }

                // Clear change tracker and continue to next retry
                DatabaseContext.ChangeTracker.Clear();
            }
            catch (Exception ex)
            {
                return Result.Fail($"خطا در ذخیره‌سازی: {ex.Message}");
            }
        }

        return Result.Fail("تلاش‌های متعدد برای ذخیره‌سازی ناموفق بود.");
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }

}

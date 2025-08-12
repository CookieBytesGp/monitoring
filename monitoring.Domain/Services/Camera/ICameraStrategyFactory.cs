using FluentResults;
using Monitoring.Domain.Aggregates.Camera;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monitoring.Domain.Services.Camera
{
    /// <summary>
    /// فکتوری برای انتخاب و ایجاد استراتژی مناسب اتصال دوربین
    /// </summary>
    public interface ICameraStrategyFactory
    {
        /// <summary>
        /// دریافت بهترین استراتژی برای دوربین مشخص شده
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <returns>مناسب‌ترین استراتژی اتصال</returns>
        Task<Result<ICameraConnectionStrategy>> GetBestStrategyAsync(Monitoring.Domain.Aggregates.Camera.Camera camera);

        /// <summary>
        /// دریافت استراتژی بر اساس نام
        /// </summary>
        /// <param name="strategyName">نام استراتژی</param>
        /// <returns>استراتژی مورد نظر</returns>
        Result<ICameraConnectionStrategy> GetStrategyByName(string strategyName);

        /// <summary>
        /// دریافت تمام استراتژی‌های موجود
        /// </summary>
        /// <returns>لیست تمام استراتژی‌ها</returns>
        IEnumerable<ICameraConnectionStrategy> GetAllStrategies();

        /// <summary>
        /// دریافت استراتژی‌های پشتیبان شده توسط دوربین
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <returns>لیست استراتژی‌های سازگار</returns>
        Task<Result<IEnumerable<ICameraConnectionStrategy>>> GetSupportedStrategiesAsync(Monitoring.Domain.Aggregates.Camera.Camera camera);

        /// <summary>
        /// تست تمام استراتژی‌ها برای یافتن بهترین گزینه
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <returns>لیست استراتژی‌های قابل اتصال مرتب شده بر اساس اولویت</returns>
        Task<Result<IEnumerable<ICameraConnectionStrategy>>> TestAllStrategiesAsync(Monitoring.Domain.Aggregates.Camera.Camera camera);

        /// <summary>
        /// ثبت استراتژی جدید
        /// </summary>
        /// <param name="strategy">استراتژی جدید</param>
        void RegisterStrategy(ICameraConnectionStrategy strategy);

        /// <summary>
        /// حذف استراتژی
        /// </summary>
        /// <param name="strategyName">نام استراتژی</param>
        bool UnregisterStrategy(string strategyName);
    }
}

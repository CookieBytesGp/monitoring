using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monitoring.Domain.Aggregates.Camera;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;

namespace Monitoring.Domain.Services.Camera
{
    /// <summary>
    /// Domain Service برای مدیریت استراتژی‌های اتصال به دوربین
    /// این سرویس منطق انتخاب بهترین استراتژی را پیاده‌سازی می‌کند
    /// </summary>
    public interface ICameraConnectionDomainService
    {
        /// <summary>
        /// انتخاب بهترین استراتژی برای دوربین بر اساس خصوصیات آن
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <param name="availableStrategies">لیست استراتژی‌های در دسترس</param>
        /// <returns>بهترین استراتژی</returns>
        Result<ICameraConnectionStrategy> SelectOptimalStrategy(
            Monitoring.Domain.Aggregates.Camera.Camera camera, 
            IEnumerable<ICameraConnectionStrategy> availableStrategies);

        /// <summary>
        /// دریافت لیست استراتژی‌های پشتیبانی شده برای دوربین به ترتیب اولویت
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <param name="availableStrategies">لیست استراتژی‌های در دسترس</param>
        /// <returns>لیست استراتژی‌های مرتب شده</returns>
        Result<List<ICameraConnectionStrategy>> GetSupportedStrategies(
            Monitoring.Domain.Aggregates.Camera.Camera camera, 
            IEnumerable<ICameraConnectionStrategy> availableStrategies);

        /// <summary>
        /// بررسی سازگاری دوربین با استراتژی مشخص
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <param name="strategy">استراتژی مورد بررسی</param>
        /// <returns>نتیجه بررسی سازگاری</returns>
        Result<bool> IsStrategyCompatible(Monitoring.Domain.Aggregates.Camera.Camera camera, ICameraConnectionStrategy strategy);

        /// <summary>
        /// تست اتصال با چندین استراتژی و انتخاب بهترین
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <param name="strategies">لیست استراتژی‌ها برای تست</param>
        /// <returns>بهترین استراتژی کارکرد</returns>
        Task<Result<ICameraConnectionStrategy>> TestAndSelectBestStrategy(
            Monitoring.Domain.Aggregates.Camera.Camera camera, 
            IEnumerable<ICameraConnectionStrategy> strategies);

        /// <summary>
        /// اعتبارسنجی تنظیمات اتصال دوربین
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Result ValidateCameraConnectionSettings(Monitoring.Domain.Aggregates.Camera.Camera camera);

        /// <summary>
        /// تولید URL های پیش‌فرض برای دوربین بر اساس نوع آن
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <returns>لیست URL های پیشنهادی</returns>
        Result<Dictionary<string, string>> GenerateDefaultUrls(Monitoring.Domain.Aggregates.Camera.Camera camera);
    }
}

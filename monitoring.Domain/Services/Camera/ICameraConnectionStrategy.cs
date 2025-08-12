using FluentResults;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Monitoring.Domain.Aggregates.Camera;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;

namespace Monitoring.Domain.Services.Camera
{
    /// <summary>
    /// Domain Service Interface برای استراتژی‌های مختلف اتصال به دوربین
    /// این interface در Domain قرار دارد تا اصول DDD رعایت شود
    /// </summary>
    public interface ICameraConnectionStrategy
    {
        /// <summary>
        /// نام استراتژی (مثل RTSP, ONVIF, Hikvision)
        /// </summary>
        string StrategyName { get; }

        /// <summary>
        /// اولویت استراتژی (عدد کمتر = اولویت بالاتر)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// بررسی اینکه آیا این استراتژی از دوربین مورد نظر پشتیبانی می‌کند یا خیر
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <returns>true اگر پشتیبانی شود</returns>
        bool SupportsCamera(Monitoring.Domain.Aggregates.Camera.Camera camera);

        /// <summary>
        /// تست اتصال به دوربین
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <returns>نتیجه تست اتصال</returns>
        Task<Result<bool>> TestConnectionAsync(Monitoring.Domain.Aggregates.Camera.Camera camera);

        /// <summary>
        /// دریافت URL استریم با کیفیت مشخص
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <param name="quality">کیفیت استریم (high, medium, low)</param>
        /// <returns>URL استریم</returns>
        Task<Result<string>> GetStreamUrlAsync(Monitoring.Domain.Aggregates.Camera.Camera camera, string quality = "high");

        /// <summary>
        /// گرفتن عکس از دوربین
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <returns>آرایه بایت عکس</returns>
        Task<Result<byte[]>> CaptureSnapshotAsync(Monitoring.Domain.Aggregates.Camera.Camera camera);

        /// <summary>
        /// برقراری اتصال به دوربین و دریافت اطلاعات اتصال
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <returns>اطلاعات اتصال</returns>
        Task<Result<CameraConnectionInfo>> ConnectAsync(Monitoring.Domain.Aggregates.Camera.Camera camera);

        /// <summary>
        /// قطع اتصال از دوربین
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <returns>نتیجه قطع اتصال</returns>
        Task<Result<bool>> DisconnectAsync(Monitoring.Domain.Aggregates.Camera.Camera camera);

        /// <summary>
        /// دریافت اطلاعات قابلیت‌های دوربین
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <returns>لیست قابلیت‌ها</returns>
        Task<Result<List<string>>> GetCapabilitiesAsync(Monitoring.Domain.Aggregates.Camera.Camera camera);

        /// <summary>
        /// تنظیم کیفیت استریم دوربین
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <param name="quality">کیفیت مورد نظر</param>
        /// <returns>نتیجه تنظیم</returns>
        Task<Result<bool>> SetStreamQualityAsync(Monitoring.Domain.Aggregates.Camera.Camera camera, string quality);

        /// <summary>
        /// دریافت وضعیت فعلی دوربین
        /// </summary>
        /// <param name="camera">دوربین مورد نظر</param>
        /// <returns>وضعیت دوربین</returns>
        Task<Result<Dictionary<string, object>>> GetCameraStatusAsync(Monitoring.Domain.Aggregates.Camera.Camera camera);
    }
}

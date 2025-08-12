using FluentResults;
using Microsoft.Extensions.Logging;
using Monitoring.Domain.Aggregates.Camera;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Monitoring.Infrastructure.Camera.Strategies
{
    /// <summary>
    /// کلاس پایه برای همه استراتژی‌های اتصال دوربین
    /// شامل متدهای مشترک و helper methods
    /// </summary>
    public abstract class BaseCameraStrategy
    {
        protected readonly ILogger _logger;
        protected readonly HttpClient _httpClient;

        protected BaseCameraStrategy(ILogger logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        #region Protected Helper Methods

        /// <summary>
        /// اعتبارسنجی پایه دوربین
        /// </summary>
        protected virtual Result ValidateCamera(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            if (camera == null)
                return Result.Fail("Camera cannot be null");

            if (camera.Network == null)
                return Result.Fail("Camera network configuration is missing");

            if (string.IsNullOrWhiteSpace(camera.Network.IpAddress))
                return Result.Fail("Camera IP address is required");

            if (camera.Network.Port <= 0 || camera.Network.Port > 65535)
                return Result.Fail("Camera port must be between 1 and 65535");

            return Result.Ok();
        }

        /// <summary>
        /// اضافه کردن header های authentication
        /// </summary>
        protected virtual void AddAuthenticationHeaders(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            if (camera.Network.HasCredentials)
            {
                var credentials = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{camera.Network.Username}:{camera.Network.Password}"));
                
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            }
        }

        /// <summary>
        /// پنهان کردن اطلاعات حساس در URL برای لاگ
        /// </summary>
        protected virtual string MaskCredentials(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;

            try
            {
                if (url.Contains("://") && url.Contains("@"))
                {
                    var protocolIndex = url.IndexOf("://") + 3;
                    var atIndex = url.IndexOf("@", protocolIndex);
                    
                    if (atIndex > protocolIndex)
                    {
                        var beforeCredentials = url.Substring(0, protocolIndex);
                        var afterAt = url.Substring(atIndex);
                        var credentials = url.Substring(protocolIndex, atIndex - protocolIndex);
                        
                        if (credentials.Contains(":"))
                        {
                            var colonIndex = credentials.IndexOf(":");
                            var username = credentials.Substring(0, colonIndex);
                            return $"{beforeCredentials}{username}:***{afterAt}";
                        }
                    }
                }
                return url;
            }
            catch
            {
                return "***MASKED_URL***";
            }
        }

        /// <summary>
        /// تولید timeout مناسب بر اساس نوع عملیات
        /// </summary>
        protected virtual TimeSpan GetTimeoutForOperation(string operation)
        {
            return operation.ToLower() switch
            {
                "ping" => TimeSpan.FromSeconds(3),
                "tcp" => TimeSpan.FromSeconds(5),
                "http" => TimeSpan.FromSeconds(10),
                "snapshot" => TimeSpan.FromSeconds(15),
                "stream" => TimeSpan.FromSeconds(30),
                _ => TimeSpan.FromSeconds(10)
            };
        }

        /// <summary>
        /// تولید User-Agent مناسب برای درخواست‌های HTTP
        /// </summary>
        protected virtual string GetUserAgent()
        {
            return "MonitoringSystem/1.0 (Camera Management)";
        }

        /// <summary>
        /// لاگ خطای استاندارد
        /// </summary>
        protected virtual void LogError(Exception ex, string operation, Monitoring.Domain.Aggregates.Camera.Camera camera, params object[] additionalParams)
        {
            _logger.LogError(ex, 
                "Error during {Operation} for camera {CameraName} ({CameraId}). Additional info: {@AdditionalParams}", 
                operation, camera.Name, camera.Id, additionalParams);
        }

        /// <summary>
        /// لاگ موفقیت استاندارد
        /// </summary>
        protected virtual void LogSuccess(string operation, Monitoring.Domain.Aggregates.Camera.Camera camera, params object[] additionalParams)
        {
            _logger.LogInformation(
                "Successfully completed {Operation} for camera {CameraName} ({CameraId}). Additional info: {@AdditionalParams}", 
                operation, camera.Name, camera.Id, additionalParams);
        }

        /// <summary>
        /// لاگ هشدار استاندارد
        /// </summary>
        protected virtual void LogWarning(string operation, Monitoring.Domain.Aggregates.Camera.Camera camera, string message, params object[] additionalParams)
        {
            _logger.LogWarning(
                "Warning during {Operation} for camera {CameraName} ({CameraId}): {Message}. Additional info: {@AdditionalParams}", 
                operation, camera.Name, camera.Id, message, additionalParams);
        }

        /// <summary>
        /// تولید نام منحصر به فرد برای session یا transaction
        /// </summary>
        protected virtual string GenerateSessionId()
        {
            return Guid.NewGuid().ToString("N")[..8];
        }

        /// <summary>
        /// بررسی اینکه آیا IP آدرس در محدوده محلی است
        /// </summary>
        protected virtual bool IsLocalIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) return false;

            return ipAddress.StartsWith("192.168.") ||
                   ipAddress.StartsWith("10.") ||
                   ipAddress.StartsWith("172.16.") ||
                   ipAddress.StartsWith("172.17.") ||
                   ipAddress.StartsWith("172.18.") ||
                   ipAddress.StartsWith("172.19.") ||
                   ipAddress.StartsWith("172.2") ||
                   ipAddress.StartsWith("172.3") ||
                   ipAddress.StartsWith("127.") ||
                   ipAddress == "localhost";
        }

        /// <summary>
        /// تشخیص نوع دوربین بر اساس پورت و سایر ویژگی‌ها
        /// </summary>
        protected virtual string DetectCameraType(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            var port = camera.Network.Port;
            var brand = camera.Configuration?.Brand?.ToLower() ?? "";

            return port switch
            {
                554 => "RTSP",
                80 or 8080 => "HTTP/ONVIF",
                1935 => "RTMP",
                8554 => "RTSP_ALTERNATE",
                _ when brand.Contains("hikvision") => "HIKVISION",
                _ when brand.Contains("dahua") => "DAHUA",
                _ when brand.Contains("axis") => "AXIS",
                _ => "GENERIC_IP"
            };
        }

        #endregion

        #region Protected Abstract/Virtual Methods

        /// <summary>
        /// بررسی سلامت اتصال - باید در کلاس‌های مشتق پیاده‌سازی شود
        /// </summary>
        protected abstract Task<Result<bool>> PerformHealthCheckAsync(Monitoring.Domain.Aggregates.Camera.Camera camera);

        /// <summary>
        /// تنظیمات خاص استراتژی - می‌تواند در کلاس‌های مشتق override شود
        /// </summary>
        protected virtual Task<Result> ConfigureStrategySpecificSettingsAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            return Task.FromResult(Result.Ok());
        }

        #endregion
    }
}

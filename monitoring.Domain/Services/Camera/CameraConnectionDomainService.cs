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
    /// پیاده‌سازی Domain Service برای مدیریت استراتژی‌های اتصال به دوربین
    /// </summary>
    public class CameraConnectionDomainService : ICameraConnectionDomainService
    {
        public Result<ICameraConnectionStrategy> SelectOptimalStrategy(
            Monitoring.Domain.Aggregates.Camera.Camera camera, 
            IEnumerable<ICameraConnectionStrategy> availableStrategies)
        {
            if (camera == null)
                return Result.Fail<ICameraConnectionStrategy>("Camera cannot be null");

            if (availableStrategies == null || !availableStrategies.Any())
                return Result.Fail<ICameraConnectionStrategy>("No strategies available");

            // فیلتر کردن استراتژی‌های پشتیبانی شده
            var supportedStrategies = availableStrategies
                .Where(s => s.SupportsCamera(camera))
                .OrderBy(s => s.Priority)
                .ToList();

            if (!supportedStrategies.Any())
                return Result.Fail<ICameraConnectionStrategy>("No compatible strategy found for this camera");

            // انتخاب بر اساس اولویت و نوع دوربین
            var optimalStrategy = SelectBasedOnCameraCharacteristics(camera, supportedStrategies);
            
            return Result.Ok(optimalStrategy);
        }

        public Result<List<ICameraConnectionStrategy>> GetSupportedStrategies(
            Monitoring.Domain.Aggregates.Camera.Camera camera, 
            IEnumerable<ICameraConnectionStrategy> availableStrategies)
        {
            if (camera == null)
                return Result.Fail<List<ICameraConnectionStrategy>>("Camera cannot be null");

            if (availableStrategies == null)
                return Result.Fail<List<ICameraConnectionStrategy>>("Available strategies cannot be null");

            var supportedStrategies = availableStrategies
                .Where(s => s.SupportsCamera(camera))
                .OrderBy(s => s.Priority)
                .ToList();

            return Result.Ok(supportedStrategies);
        }

        public Result<bool> IsStrategyCompatible(Monitoring.Domain.Aggregates.Camera.Camera camera, ICameraConnectionStrategy strategy)
        {
            if (camera == null)
                return Result.Fail<bool>("Camera cannot be null");

            if (strategy == null)
                return Result.Fail<bool>("Strategy cannot be null");

            try
            {
                var isCompatible = strategy.SupportsCamera(camera);
                return Result.Ok(isCompatible);
            }
            catch (Exception ex)
            {
                return Result.Fail<bool>($"Error checking compatibility: {ex.Message}");
            }
        }

        public async Task<Result<ICameraConnectionStrategy>> TestAndSelectBestStrategy(
            Monitoring.Domain.Aggregates.Camera.Camera camera, 
            IEnumerable<ICameraConnectionStrategy> strategies)
        {
            if (camera == null)
                return Result.Fail<ICameraConnectionStrategy>("Camera cannot be null");

            var supportedStrategiesResult = GetSupportedStrategies(camera, strategies);
            if (supportedStrategiesResult.IsFailed)
                return Result.Fail<ICameraConnectionStrategy>(supportedStrategiesResult.Errors);

            var supportedStrategies = supportedStrategiesResult.Value;
            if (!supportedStrategies.Any())
                return Result.Fail<ICameraConnectionStrategy>("No supported strategies found");

            // تست هر استراتژی به ترتیب اولویت
            foreach (var strategy in supportedStrategies)
            {
                try
                {
                    var testResult = await strategy.TestConnectionAsync(camera);
                    if (testResult.IsSuccess && testResult.Value)
                    {
                        return Result.Ok(strategy);
                    }
                }
                catch (Exception ex)
                {
                    // لاگ خطا و ادامه با استراتژی بعدی
                    continue;
                }
            }

            return Result.Fail<ICameraConnectionStrategy>("No strategy passed connection test");
        }

        public Result ValidateCameraConnectionSettings(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            if (camera == null)
                return Result.Fail("Camera cannot be null");

            var errors = new List<string>();

            // بررسی تنظیمات شبکه
            if (camera.Network == null)
            {
                errors.Add("Camera network configuration is missing");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(camera.Network.IpAddress))
                    errors.Add("IP Address is required");

                if (camera.Network.Port <= 0 || camera.Network.Port > 65535)
                    errors.Add("Port must be between 1 and 65535");
            }

            // بررسی نوع دوربین
            if (camera.Type == null)
                errors.Add("Camera type is required");

            // بررسی نام دوربین
            if (string.IsNullOrWhiteSpace(camera.Name))
                errors.Add("Camera name is required");

            if (errors.Any())
                return Result.Fail(errors);

            return Result.Ok();
        }

        public Result<Dictionary<string, string>> GenerateDefaultUrls(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            if (camera == null)
                return Result.Fail<Dictionary<string, string>>("Camera cannot be null");

            var validationResult = ValidateCameraConnectionSettings(camera);
            if (validationResult.IsFailed)
                return Result.Fail<Dictionary<string, string>>(validationResult.Errors);

            var urls = new Dictionary<string, string>();

            var network = camera.Network;
            var hasAuth = !string.IsNullOrEmpty(network.Username);
            var credentials = hasAuth ? $"{network.Username}:{network.Password}@" : "";

            // تولید URL های مختلف بر اساس نوع دوربین
            switch (camera.Type.Name.ToLower())
            {
                case "ip":
                case "ipcamera":
                    urls["rtsp_main"] = $"rtsp://{credentials}{network.IpAddress}:{network.Port}/stream1";
                    urls["rtsp_sub"] = $"rtsp://{credentials}{network.IpAddress}:{network.Port}/stream2";
                    urls["http_stream"] = $"http://{credentials}{network.IpAddress}:{network.Port}/videostream.cgi";
                    urls["snapshot"] = $"http://{credentials}{network.IpAddress}:{network.Port}/snapshot.jpg";
                    break;

                case "onvif":
                    urls["rtsp_main"] = $"rtsp://{credentials}{network.IpAddress}:554/onvif1";
                    urls["rtsp_sub"] = $"rtsp://{credentials}{network.IpAddress}:554/onvif2";
                    urls["snapshot"] = $"http://{credentials}{network.IpAddress}/onvif/snapshot";
                    break;

                case "usb":
                    urls["device"] = "/dev/video0";
                    urls["fallback_device"] = "/dev/video1";
                    break;

                default:
                    urls["rtsp_default"] = $"rtsp://{credentials}{network.IpAddress}:{network.Port}/stream";
                    urls["http_default"] = $"http://{credentials}{network.IpAddress}:{network.Port}/stream";
                    break;
            }

            return Result.Ok(urls);
        }

        #region Private Methods

        private ICameraConnectionStrategy SelectBasedOnCameraCharacteristics(
            Monitoring.Domain.Aggregates.Camera.Camera camera, 
            List<ICameraConnectionStrategy> supportedStrategies)
        {
            // اولویت‌بندی بر اساس نوع دوربین و برند
            
            // اگر برند مشخص است، استراتژی مخصوص آن برند را ترجیح می‌دهیم
            if (camera.Configuration?.Brand != null)
            {
                var brandStrategy = supportedStrategies
                    .FirstOrDefault(s => s.StrategyName.ToLower().Contains(camera.Configuration.Brand.ToLower()));
                
                if (brandStrategy != null)
                    return brandStrategy;
            }

            // اگر دوربین ONVIF پشتیبانی می‌کند، آن را ترجیح می‌دهیم
            var onvifStrategy = supportedStrategies
                .FirstOrDefault(s => s.StrategyName.ToLower().Contains("onvif"));
            
            if (onvifStrategy != null && IsOnvifLikelySupported(camera))
                return onvifStrategy;

            // در غیر این صورت اولین استراتژی با کمترین اولویت
            return supportedStrategies.First();
        }

        private bool IsOnvifLikelySupported(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            // بررسی‌های ساده برای تشخیص پشتیبانی احتمالی ONVIF
            return camera.Type.Name.ToLower().Contains("ip") &&
                   (camera.Network.Port == 80 || camera.Network.Port == 8080 || camera.Network.Port == 554);
        }

        #endregion
    }
}

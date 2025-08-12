using FluentResults;
using Microsoft.Extensions.Logging;
using Monitoring.Domain.Aggregates.Camera;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;
using Monitoring.Domain.Services.Camera;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Monitoring.Infrastructure.Camera.Strategies
{
    /// <summary>
    /// استراتژی اتصال USB - برای دوربین‌های USB محلی
    /// </summary>
    public class USBCameraStrategy : BaseCameraStrategy, ICameraConnectionStrategy
    {
        private readonly Dictionary<int, string> _devicePaths;

        public USBCameraStrategy(ILogger<USBCameraStrategy> logger, HttpClient httpClient) 
            : base(logger, httpClient)
        {
            _devicePaths = new Dictionary<int, string>();
            InitializeDevicePaths();
        }

        #region Properties

        public string StrategyName => "USB";
        public int Priority => 5; // اولویت پایین‌تر

        #endregion

        #region Public Methods

        public bool SupportsCamera(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            if (camera?.Type == null) return false;

            return camera.Type.Name.ToLower().Contains("usb") ||
                   camera.Type.Name.ToLower().Contains("webcam") ||
                   camera.Type.Name.ToLower().Contains("local");
        }

        public async Task<Result<bool>> TestConnectionAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Testing USB connection for camera {CameraName}", camera.Name);

                if (!SupportsCamera(camera))
                    return Result.Fail<bool>("Camera is not supported by USB strategy");

                var healthCheckResult = await PerformHealthCheckAsync(camera);
                if (healthCheckResult.IsFailed)
                    return healthCheckResult;

                LogSuccess("TestConnection", camera);
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                LogError(ex, "TestConnection", camera);
                return Result.Fail<bool>($"USB connection test failed: {ex.Message}");
            }
        }

        public async Task<Result<string>> GetStreamUrlAsync(Monitoring.Domain.Aggregates.Camera.Camera camera, string quality = "high")
        {
            try
            {
                if (!SupportsCamera(camera))
                    return Result.Fail<string>("Camera is not supported by USB strategy");

                var devicePath = await DiscoverDevicePathAsync(camera);
                if (string.IsNullOrEmpty(devicePath))
                    return Result.Fail<string>("No USB camera device found");

                // برای USB cameras، stream URL معمولاً device path است
                var streamUrl = $"usb://{devicePath}?quality={quality}";
                
                _logger.LogDebug("Generated USB stream URL for camera {CameraName}: {StreamUrl}", 
                    camera.Name, streamUrl);

                return Result.Ok(streamUrl);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetStreamUrl", camera, new { Quality = quality });
                return Result.Fail<string>($"Failed to generate USB stream URL: {ex.Message}");
            }
        }

        public async Task<Result<byte[]>> CaptureSnapshotAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Capturing USB snapshot from camera {CameraName}", camera.Name);

                var devicePath = await DiscoverDevicePathAsync(camera);
                if (string.IsNullOrEmpty(devicePath))
                    return Result.Fail<byte[]>("No USB camera device found");

                // شبیه‌سازی capture از USB device
                // در پیاده‌سازی واقعی، باید از OpenCV یا DirectShow استفاده شود
                var simulatedImage = await SimulateCaptureAsync(devicePath);
                
                if (simulatedImage != null && simulatedImage.Length > 0)
                {
                    LogSuccess("CaptureSnapshot", camera, new { Size = simulatedImage.Length });
                    return Result.Ok(simulatedImage);
                }
                else
                {
                    return Result.Fail<byte[]>("Failed to capture image from USB device");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "CaptureSnapshot", camera);
                return Result.Fail<byte[]>($"USB snapshot capture failed: {ex.Message}");
            }
        }

        public async Task<Result<CameraConnectionInfo>> ConnectAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Establishing USB connection for camera {CameraName}", camera.Name);

                var testResult = await TestConnectionAsync(camera);
                if (testResult.IsFailed)
                    return Result.Fail<CameraConnectionInfo>(testResult.Errors);

                var devicePath = await DiscoverDevicePathAsync(camera);
                var streamUrl = $"usb://{devicePath}";

                var connectionInfoResult = CameraConnectionInfo.Create(
                    streamUrl: streamUrl,
                    snapshotUrl: streamUrl, // برای USB همان device path
                    isConnected: true,
                    connectionType: StrategyName
                );

                if (connectionInfoResult.IsFailed)
                    return Result.Fail<CameraConnectionInfo>(connectionInfoResult.Errors);

                var connectionInfo = connectionInfoResult.Value
                    .AddInfo("protocol", "USB")
                    .AddInfo("device_path", devicePath)
                    .AddInfo("interface_type", "DirectShow/V4L2");

                LogSuccess("Connect", camera);
                return Result.Ok(connectionInfo);
            }
            catch (Exception ex)
            {
                LogError(ex, "Connect", camera);
                return Result.Fail<CameraConnectionInfo>($"USB connection failed: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DisconnectAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Disconnecting USB connection for camera {CameraName}", camera.Name);

                // در پیاده‌سازی واقعی، باید device handle ها آزاد شوند
                await Task.Delay(100); // Simulate cleanup

                LogSuccess("Disconnect", camera);
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                LogError(ex, "Disconnect", camera);
                return Result.Fail<bool>($"USB disconnection failed: {ex.Message}");
            }
        }

        public async Task<Result<List<string>>> GetCapabilitiesAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var capabilities = new List<string>
                {
                    "usb_streaming",
                    "local_capture",
                    "directshow_support",
                    "multiple_resolutions",
                    "frame_rate_control"
                };

                // اضافه کردن قابلیت‌های خاص سیستم‌عامل
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    capabilities.Add("directshow");
                    capabilities.Add("media_foundation");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    capabilities.Add("v4l2");
                    capabilities.Add("uvc_driver");
                }

                await Task.CompletedTask;
                return Result.Ok(capabilities);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetCapabilities", camera);
                return Result.Fail<List<string>>($"Failed to get USB capabilities: {ex.Message}");
            }
        }

        public async Task<Result<bool>> SetStreamQualityAsync(Monitoring.Domain.Aggregates.Camera.Camera camera, string quality)
        {
            try
            {
                _logger.LogInformation("Setting USB stream quality to {Quality} for camera {CameraName}", 
                    quality, camera.Name);

                // برای USB cameras، کیفیت معمولاً از طریق resolution و frame rate تنظیم می‌شود
                await Task.CompletedTask;
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                LogError(ex, "SetStreamQuality", camera, new { Quality = quality });
                return Result.Fail<bool>($"Failed to set USB stream quality: {ex.Message}");
            }
        }

        public async Task<Result<Dictionary<string, object>>> GetCameraStatusAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var devicePath = await DiscoverDevicePathAsync(camera);
                var isConnected = !string.IsNullOrEmpty(devicePath) && await CheckDeviceAvailabilityAsync(devicePath);

                var status = new Dictionary<string, object>
                {
                    ["strategy"] = StrategyName,
                    ["protocol"] = "USB",
                    ["device_path"] = devicePath ?? "unknown",
                    ["is_connected"] = isConnected,
                    ["interface_type"] = GetInterfaceType(),
                    ["supported_qualities"] = new[] { "high", "medium", "low" },
                    ["last_check"] = DateTime.UtcNow
                };

                return Result.Ok(status);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetCameraStatus", camera);
                return Result.Fail<Dictionary<string, object>>($"Failed to get USB camera status: {ex.Message}");
            }
        }

        #endregion

        #region Protected Override Methods

        protected override async Task<Result<bool>> PerformHealthCheckAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var devicePath = await DiscoverDevicePathAsync(camera);
                if (string.IsNullOrEmpty(devicePath))
                    return Result.Fail<bool>("No USB camera device found");

                var isAvailable = await CheckDeviceAvailabilityAsync(devicePath);
                if (!isAvailable)
                    return Result.Fail<bool>($"USB device {devicePath} is not available");

                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                LogError(ex, "PerformHealthCheck", camera);
                return Result.Fail<bool>($"USB health check failed: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods

        private void InitializeDevicePaths()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows device paths
                for (int i = 0; i < 10; i++)
                {
                    _devicePaths[i] = $"DirectShow:{i}";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux device paths
                for (int i = 0; i < 10; i++)
                {
                    _devicePaths[i] = $"/dev/video{i}";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS device paths
                for (int i = 0; i < 10; i++)
                {
                    _devicePaths[i] = $"AVFoundation:{i}";
                }
            }
        }

        private async Task<string> DiscoverDevicePathAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                // اگر در Configuration مسیر device مشخص شده، از آن استفاده کن
                if (camera.Configuration?.AdditionalSettings?.ContainsKey("device_path") == true)
                {
                    return camera.Configuration.AdditionalSettings["device_path"];
                }

                // در غیر این صورت، device های موجود را جستجو کن
                foreach (var devicePath in _devicePaths.Values)
                {
                    if (await CheckDeviceAvailabilityAsync(devicePath))
                    {
                        return devicePath;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error discovering USB device path for camera {CameraName}", camera.Name);
                return null;
            }
        }

        private async Task<bool> CheckDeviceAvailabilityAsync(string devicePath)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // بررسی وجود فایل device در Linux
                    if (devicePath.StartsWith("/dev/video"))
                    {
                        return File.Exists(devicePath);
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // در Windows باید از DirectShow API استفاده شود
                    // فعلاً شبیه‌سازی می‌کنیم
                    await Task.Delay(10);
                    return devicePath.StartsWith("DirectShow:");
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<byte[]> SimulateCaptureAsync(string devicePath)
        {
            try
            {
                // شبیه‌سازی capture تصویر
                // در پیاده‌سازی واقعی باید از OpenCV، DirectShow، V4L2 استفاده شود
                
                await Task.Delay(100); // شبیه‌سازی زمان capture

                // ایجاد یک تصویر dummy JPEG
                var dummyJpeg = new byte[] 
                {
                    0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46,
                    0x00, 0x01, 0x01, 0x01, 0x00, 0x48, 0x00, 0x48, 0x00, 0x00,
                    0xFF, 0xD9 // End of JPEG
                };

                _logger.LogDebug("Simulated capture from USB device {DevicePath}", devicePath);
                return dummyJpeg;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating capture from USB device {DevicePath}", devicePath);
                return null;
            }
        }

        private string GetInterfaceType()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "DirectShow/MediaFoundation";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "V4L2/UVC";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "AVFoundation";
            else
                return "Unknown";
        }

        #endregion
    }
}

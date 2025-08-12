using FluentResults;
using Microsoft.Extensions.Logging;
using Monitoring.Domain.Aggregates.Camera;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;
using Monitoring.Domain.Services.Camera;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Monitoring.Infrastructure.Camera.Strategies
{
    /// <summary>
    /// استراتژی اتصال HTTP - برای دوربین‌هایی که از HTTP streaming پشتیبانی می‌کنند
    /// شامل MJPEG، Progressive JPEG و snapshot های HTTP
    /// </summary>
    public class HTTPCameraStrategy : BaseCameraStrategy, ICameraConnectionStrategy
    {
        private readonly Dictionary<string, string> _commonStreamPaths;
        private readonly Dictionary<string, string> _commonSnapshotPaths;

        public HTTPCameraStrategy(ILogger<HTTPCameraStrategy> logger, HttpClient httpClient) 
            : base(logger, httpClient)
        {
            _commonStreamPaths = new Dictionary<string, string>
            {
                ["high"] = "/video.cgi?resolution=high",
                ["medium"] = "/video.cgi?resolution=medium", 
                ["low"] = "/video.cgi?resolution=low",
                ["mjpeg_high"] = "/videostream.cgi?rate=0",
                ["mjpeg_medium"] = "/videostream.cgi?rate=1",
                ["mjpeg_low"] = "/videostream.cgi?rate=2"
            };

            _commonSnapshotPaths = new Dictionary<string, string>
            {
                ["snapshot"] = "/snapshot.jpg",
                ["image"] = "/image.jpg",
                ["cgi_snapshot"] = "/cgi-bin/snapshot.cgi",
                ["capture"] = "/capture.jpg",
                ["current"] = "/current.jpg"
            };
        }

        #region Properties

        public string StrategyName => "HTTP";
        public int Priority => 4; // اولویت کمتر از RTSP

        #endregion

        #region Public Methods

        public bool SupportsCamera(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            var validationResult = ValidateCamera(camera);
            if (validationResult.IsFailed) return false;

            // HTTP معمولاً برای دوربین‌های IP استفاده می‌شود
            // پورت‌های معمول: 80, 8080, 8081, 8000
            var isHttpPort = camera.Network.Port == 80 || 
                           camera.Network.Port == 8080 || 
                           camera.Network.Port == 8081 ||
                           camera.Network.Port == 8000 ||
                           (camera.Network.Port >= 8000 && camera.Network.Port <= 8999);

            return camera.Type.Name.ToLower().Contains("ip") && 
                   (isHttpPort || DetectCameraType(camera) == "HTTP/ONVIF");
        }

        public async Task<Result<bool>> TestConnectionAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Testing HTTP connection for camera {CameraName} at {IpAddress}:{Port}", 
                    camera.Name, camera.Network.IpAddress, camera.Network.Port);

                var validationResult = ValidateCamera(camera);
                if (validationResult.IsFailed)
                    return Result.Fail<bool>(validationResult.Errors);

                // مرحله 1: تست اتصال پایه
                var healthCheckResult = await PerformHealthCheckAsync(camera);
                if (healthCheckResult.IsFailed)
                    return healthCheckResult;

                // مرحله 2: تست endpoint های مختلف HTTP
                var endpointTestResult = await TestHttpEndpointsAsync(camera);
                
                LogSuccess("TestConnection", camera, new { EndpointTest = endpointTestResult });
                
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                LogError(ex, "TestConnection", camera);
                return Result.Fail<bool>($"HTTP connection test failed: {ex.Message}");
            }
        }

        public async Task<Result<string>> GetStreamUrlAsync(Monitoring.Domain.Aggregates.Camera.Camera camera, string quality = "high")
        {
            try
            {
                if (!SupportsCamera(camera))
                    return Result.Fail<string>("Camera is not supported by HTTP strategy");

                var streamUrl = await DiscoverStreamUrlAsync(camera, quality);
                
                _logger.LogDebug("Generated HTTP stream URL for camera {CameraName}: {StreamUrl}", 
                    camera.Name, MaskCredentials(streamUrl));

                return Result.Ok(streamUrl);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetStreamUrl", camera, new { Quality = quality });
                return Result.Fail<string>($"Failed to generate HTTP stream URL: {ex.Message}");
            }
        }

        public async Task<Result<byte[]>> CaptureSnapshotAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Capturing HTTP snapshot from camera {CameraName}", camera.Name);

                var snapshotUrl = await DiscoverSnapshotUrlAsync(camera);
                if (string.IsNullOrEmpty(snapshotUrl))
                    return Result.Fail<byte[]>("No valid snapshot URL found");

                AddAuthenticationHeaders(camera);
                ConfigureHttpClientForSnapshot();

                var response = await _httpClient.GetAsync(snapshotUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    
                    // بررسی اینکه آیا محتوا واقعاً تصویر است
                    if (!IsValidImageContent(imageBytes))
                        return Result.Fail<byte[]>("Invalid image content received");
                    
                    LogSuccess("CaptureSnapshot", camera, new { Size = imageBytes.Length });
                    return Result.Ok(imageBytes);
                }
                else
                {
                    LogWarning("CaptureSnapshot", camera, $"HTTP request failed with status: {response.StatusCode}");
                    return Result.Fail<byte[]>($"HTTP request failed with status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "CaptureSnapshot", camera);
                return Result.Fail<byte[]>($"HTTP snapshot capture failed: {ex.Message}");
            }
        }

        public async Task<Result<CameraConnectionInfo>> ConnectAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Establishing HTTP connection for camera {CameraName}", camera.Name);

                // تست اتصال
                var testResult = await TestConnectionAsync(camera);
                if (testResult.IsFailed)
                    return Result.Fail<CameraConnectionInfo>(testResult.Errors);

                // کشف URL های مختلف
                var streamUrl = await DiscoverStreamUrlAsync(camera, "high");
                var snapshotUrl = await DiscoverSnapshotUrlAsync(camera);
                var backupStreamUrl = await DiscoverStreamUrlAsync(camera, "medium");

                // ایجاد اطلاعات اتصال
                var connectionInfoResult = CameraConnectionInfo.Create(
                    streamUrl: streamUrl,
                    snapshotUrl: snapshotUrl,
                    isConnected: true,
                    connectionType: StrategyName,
                    backupStreamUrl: backupStreamUrl
                );

                if (connectionInfoResult.IsFailed)
                    return Result.Fail<CameraConnectionInfo>(connectionInfoResult.Errors);

                var connectionInfo = connectionInfoResult.Value
                    .AddInfo("protocol", "HTTP")
                    .AddInfo("streaming_type", "MJPEG")
                    .AddInfo("port", camera.Network.Port.ToString())
                    .AddInfo("authentication", camera.Network.HasCredentials ? "enabled" : "disabled");

                LogSuccess("Connect", camera);
                return Result.Ok(connectionInfo);
            }
            catch (Exception ex)
            {
                LogError(ex, "Connect", camera);
                return Result.Fail<CameraConnectionInfo>($"HTTP connection failed: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DisconnectAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Disconnecting HTTP connection for camera {CameraName}", camera.Name);

                // HTTP connections are stateless, so we just cleanup any resources
                CleanupHttpClient();
                await Task.Delay(50); // Simulate cleanup work

                LogSuccess("Disconnect", camera);
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                LogError(ex, "Disconnect", camera);
                return Result.Fail<bool>($"HTTP disconnection failed: {ex.Message}");
            }
        }

        public async Task<Result<List<string>>> GetCapabilitiesAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var capabilities = new List<string>
                {
                    "http_streaming",
                    "mjpeg_streaming",
                    "snapshot_capture",
                    "basic_authentication",
                    "digest_authentication",
                    "progressive_jpeg"
                };

                // بررسی قابلیت‌های اضافی بر اساس تست endpoint ها
                var additionalCapabilities = await DiscoverAdditionalCapabilitiesAsync(camera);
                capabilities.AddRange(additionalCapabilities);

                return Result.Ok(capabilities);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetCapabilities", camera);
                return Result.Fail<List<string>>($"Failed to get HTTP capabilities: {ex.Message}");
            }
        }

        public async Task<Result<bool>> SetStreamQualityAsync(Monitoring.Domain.Aggregates.Camera.Camera camera, string quality)
        {
            try
            {
                _logger.LogInformation("Setting HTTP stream quality to {Quality} for camera {CameraName}", 
                    quality, camera.Name);

                // HTTP quality is usually managed by different endpoint paths
                var streamUrl = await DiscoverStreamUrlAsync(camera, quality);
                var isValid = !string.IsNullOrEmpty(streamUrl);

                return Result.Ok(isValid);
            }
            catch (Exception ex)
            {
                LogError(ex, "SetStreamQuality", camera, new { Quality = quality });
                return Result.Fail<bool>($"Failed to set HTTP stream quality: {ex.Message}");
            }
        }

        public async Task<Result<Dictionary<string, object>>> GetCameraStatusAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var status = new Dictionary<string, object>
                {
                    ["strategy"] = StrategyName,
                    ["protocol"] = "HTTP",
                    ["ip_address"] = camera.Network.IpAddress,
                    ["port"] = camera.Network.Port,
                    ["has_authentication"] = camera.Network.HasCredentials,
                    ["streaming_type"] = "MJPEG/Progressive JPEG",
                    ["supported_qualities"] = new[] { "high", "medium", "low" }
                };

                // تست اتصال سریع
                var healthCheck = await PerformHealthCheckAsync(camera);
                status["is_healthy"] = healthCheck.IsSuccess;
                status["last_check"] = DateTime.UtcNow;

                // اطلاعات اضافی از تست endpoint ها
                var endpointInfo = await GetEndpointInformationAsync(camera);
                status["available_endpoints"] = endpointInfo;

                return Result.Ok(status);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetCameraStatus", camera);
                return Result.Fail<Dictionary<string, object>>($"Failed to get HTTP camera status: {ex.Message}");
            }
        }

        #endregion

        #region Protected Override Methods

        protected override async Task<Result<bool>> PerformHealthCheckAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var baseUrl = GenerateBaseUrl(camera);
                AddAuthenticationHeaders(camera);

                // تست endpoint های مختلف
                var endpoints = new[] { "/", "/index.html", "/cgi-bin/", "/video.cgi" };
                
                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var testUrl = $"{baseUrl}{endpoint}";
                        var response = await _httpClient.GetAsync(testUrl);
                        
                        // موفقیت یا Unauthorized (که نشان دهنده وجود سرویس است) قابل قبول است
                        if (response.IsSuccessStatusCode || 
                            response.StatusCode == HttpStatusCode.Unauthorized ||
                            response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return Result.Ok(true);
                        }
                    }
                    catch
                    {
                        // ادامه با endpoint بعدی
                        continue;
                    }
                }

                return Result.Fail<bool>("No responsive HTTP endpoints found");
            }
            catch (Exception ex)
            {
                LogError(ex, "PerformHealthCheck", camera);
                return Result.Fail<bool>($"HTTP health check failed: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods

        private async Task<string> DiscoverStreamUrlAsync(Monitoring.Domain.Aggregates.Camera.Camera camera, string quality)
        {
            var baseUrl = GenerateBaseUrl(camera);
            AddAuthenticationHeaders(camera);

            // تست path های مختلف برای stream
            var testPaths = new List<string>();
            
            // اضافه کردن path های مربوط به کیفیت درخواستی
            if (_commonStreamPaths.ContainsKey(quality))
                testPaths.Add(_commonStreamPaths[quality]);
            
            if (_commonStreamPaths.ContainsKey($"mjpeg_{quality}"))
                testPaths.Add(_commonStreamPaths[$"mjpeg_{quality}"]);

            // path های عمومی
            testPaths.AddRange(new[]
            {
                "/video",
                "/stream",
                "/live",
                "/mjpeg",
                "/videostream.cgi",
                "/video.cgi",
                "/cgi-bin/video.cgi"
            });

            foreach (var path in testPaths)
            {
                try
                {
                    var testUrl = $"{baseUrl}{path}";
                    var response = await _httpClient.GetAsync(testUrl);
                    
                    if (response.IsSuccessStatusCode && IsStreamingContent(response))
                    {
                        return testUrl;
                    }
                }
                catch
                {
                    // ادامه با path بعدی
                    continue;
                }
            }

            // اگر هیچ path مناسبی پیدا نشد، URL پیش‌فرض را برگردان
            return $"{baseUrl}/video.cgi";
        }

        private async Task<string> DiscoverSnapshotUrlAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            var baseUrl = GenerateBaseUrl(camera);
            AddAuthenticationHeaders(camera);

            foreach (var kvp in _commonSnapshotPaths)
            {
                try
                {
                    var testUrl = $"{baseUrl}{kvp.Value}";
                    var response = await _httpClient.GetAsync(testUrl);
                    
                    if (response.IsSuccessStatusCode && IsImageContent(response))
                    {
                        return testUrl;
                    }
                }
                catch
                {
                    // ادامه با path بعدی
                    continue;
                }
            }

            // URL پیش‌فرض
            return $"{baseUrl}/snapshot.jpg";
        }

        private async Task<bool> TestHttpEndpointsAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var streamUrl = await DiscoverStreamUrlAsync(camera, "medium");
                var snapshotUrl = await DiscoverSnapshotUrlAsync(camera);

                return !string.IsNullOrEmpty(streamUrl) && !string.IsNullOrEmpty(snapshotUrl);
            }
            catch
            {
                return false;
            }
        }

        private async Task<List<string>> DiscoverAdditionalCapabilitiesAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            var capabilities = new List<string>();
            var baseUrl = GenerateBaseUrl(camera);

            // تست قابلیت‌های اضافی
            var testEndpoints = new Dictionary<string, string>
            {
                ["ptz_control"] = "/cgi-bin/ptz.cgi",
                ["audio_streaming"] = "/audio.cgi",
                ["motion_detection"] = "/cgi-bin/motion.cgi",
                ["recording"] = "/cgi-bin/record.cgi"
            };

            foreach (var kvp in testEndpoints)
            {
                try
                {
                    var testUrl = $"{baseUrl}{kvp.Value}";
                    var response = await _httpClient.GetAsync(testUrl);
                    
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        capabilities.Add(kvp.Key);
                    }
                }
                catch
                {
                    // ادامه با endpoint بعدی
                    continue;
                }
            }

            return capabilities;
        }

        private async Task<Dictionary<string, bool>> GetEndpointInformationAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            var endpoints = new Dictionary<string, bool>();
            var baseUrl = GenerateBaseUrl(camera);

            var testPaths = new[] { "/", "/video.cgi", "/snapshot.jpg", "/cgi-bin/" };

            foreach (var path in testPaths)
            {
                try
                {
                    var testUrl = $"{baseUrl}{path}";
                    var response = await _httpClient.GetAsync(testUrl);
                    endpoints[path] = response.IsSuccessStatusCode;
                }
                catch
                {
                    endpoints[path] = false;
                }
            }

            return endpoints;
        }

        private string GenerateBaseUrl(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            var network = camera.Network;
            var protocol = IsLocalIpAddress(network.IpAddress) ? "http" : "http"; // همیشه HTTP
            return $"{protocol}://{network.IpAddress}:{network.Port}";
        }

        private void ConfigureHttpClientForSnapshot()
        {
            _httpClient.Timeout = GetTimeoutForOperation("snapshot");
            
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
            }
        }

        private void CleanupHttpClient()
        {
            // پاک کردن headers authentication
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private bool IsStreamingContent(HttpResponseMessage response)
        {
            var contentType = response.Content.Headers.ContentType?.MediaType?.ToLower();
            return contentType != null && (
                contentType.Contains("multipart") ||
                contentType.Contains("video") ||
                contentType.Contains("application/octet-stream")
            );
        }

        private bool IsImageContent(HttpResponseMessage response)
        {
            var contentType = response.Content.Headers.ContentType?.MediaType?.ToLower();
            return contentType != null && contentType.Contains("image");
        }

        private bool IsValidImageContent(byte[] content)
        {
            if (content == null || content.Length < 10) return false;

            // بررسی signature های مختلف تصویر
            // JPEG: FF D8
            if (content.Length >= 2 && content[0] == 0xFF && content[1] == 0xD8)
                return true;

            // PNG: 89 50 4E 47
            if (content.Length >= 4 && content[0] == 0x89 && content[1] == 0x50 && 
                content[2] == 0x4E && content[3] == 0x47)
                return true;

            // GIF: 47 49 46
            if (content.Length >= 3 && content[0] == 0x47 && content[1] == 0x49 && content[2] == 0x46)
                return true;

            return false;
        }

        #endregion
    }
}

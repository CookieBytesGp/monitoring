using FluentResults;
using Microsoft.Extensions.Logging;
using Monitoring.Domain.Aggregates.Camera;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;
using Monitoring.Domain.Services.Camera;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Monitoring.Infrastructure.Camera.Strategies
{
    /// <summary>
    /// استراتژی اتصال RTSP - ساده‌ترین و پرکاربردترین پروتکل برای دوربین‌های IP
    /// </summary>
    public class RTSPCameraStrategy : BaseCameraStrategy, ICameraConnectionStrategy
    {
        public RTSPCameraStrategy(ILogger<RTSPCameraStrategy> logger, HttpClient httpClient) 
            : base(logger, httpClient)
        {
        }

        #region Properties

        public string StrategyName => "RTSP";
        public int Priority => 3; // اولویت متوسط (عدد کمتر = اولویت بالاتر)

        #endregion

        #region Public Methods

        public bool SupportsCamera(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            var validationResult = ValidateCamera(camera);
            if (validationResult.IsFailed) return false;

            // RTSP معمولاً برای دوربین‌های IP استفاده می‌شود
            return camera.Type.Name.ToLower().Contains("ip");
        }

        public async Task<Result<bool>> TestConnectionAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Testing RTSP connection for camera {CameraName} at {IpAddress}:{Port}", 
                    camera.Name, camera.Network.IpAddress, camera.Network.Port);

                // مرحله 1: تست Ping
                var pingResult = await TestPingAsync(camera.Network.IpAddress);
                if (!pingResult)
                {
                    _logger.LogWarning("Ping failed for camera {CameraName}", camera.Name);
                    return Result.Fail<bool>("Camera is not reachable via ping");
                }

                // مرحله 2: تست اتصال TCP به پورت RTSP
                var tcpResult = await TestTcpConnectionAsync(camera.Network.IpAddress, camera.Network.Port);
                if (!tcpResult)
                {
                    _logger.LogWarning("TCP connection failed for camera {CameraName} on port {Port}", 
                        camera.Name, camera.Network.Port);
                    return Result.Fail<bool>($"TCP connection failed on port {camera.Network.Port}");
                }

                // مرحله 3: تست HTTP (اختیاری برای snapshot)
                var httpResult = await TestHttpEndpointAsync(camera);
                
                _logger.LogInformation("RTSP connection test completed for camera {CameraName}. TCP: {TcpResult}, HTTP: {HttpResult}", 
                    camera.Name, tcpResult, httpResult);

                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing RTSP connection for camera {CameraName}", camera.Name);
                return Result.Fail<bool>($"Connection test failed: {ex.Message}");
            }
        }

        public async Task<Result<string>> GetStreamUrlAsync(Monitoring.Domain.Aggregates.Camera.Camera camera, string quality = "high")
        {
            try
            {
                if (!SupportsCamera(camera))
                    return Result.Fail<string>("Camera is not supported by RTSP strategy");

                var streamUrl = GenerateRtspUrl(camera, quality);
                
                _logger.LogDebug("Generated RTSP stream URL for camera {CameraName}: {StreamUrl}", 
                    camera.Name, MaskCredentials(streamUrl));

                return Result.Ok(streamUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating RTSP stream URL for camera {CameraName}", camera.Name);
                return Result.Fail<string>($"Failed to generate stream URL: {ex.Message}");
            }
        }

        public async Task<Result<byte[]>> CaptureSnapshotAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Capturing snapshot from camera {CameraName} via HTTP", camera.Name);

                // تولید URL snapshot
                var snapshotUrl = GenerateSnapshotUrl(camera);
                
                // اضافه کردن authentication headers
                AddAuthenticationHeaders(camera);

                // ارسال درخواست HTTP
                var response = await _httpClient.GetAsync(snapshotUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    
                    _logger.LogInformation("Successfully captured snapshot from camera {CameraName}, size: {Size} bytes", 
                        camera.Name, imageBytes.Length);
                    
                    return Result.Ok(imageBytes);
                }
                else
                {
                    _logger.LogWarning("Failed to capture snapshot from camera {CameraName}. Status: {StatusCode}", 
                        camera.Name, response.StatusCode);
                    
                    return Result.Fail<byte[]>($"HTTP request failed with status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing snapshot from camera {CameraName}", camera.Name);
                return Result.Fail<byte[]>($"Snapshot capture failed: {ex.Message}");
            }
        }

        public async Task<Result<CameraConnectionInfo>> ConnectAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Establishing RTSP connection for camera {CameraName}", camera.Name);

                // تست اتصال
                var testResult = await TestConnectionAsync(camera);
                if (testResult.IsFailed)
                    return Result.Fail<CameraConnectionInfo>(testResult.Errors);

                // تولید URL های مختلف
                var streamUrl = GenerateRtspUrl(camera, "high");
                var snapshotUrl = GenerateSnapshotUrl(camera);
                var backupStreamUrl = GenerateRtspUrl(camera, "medium");

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
                    .AddInfo("protocol", "RTSP")
                    .AddInfo("port", camera.Network.Port.ToString())
                    .AddInfo("authentication", camera.Network.HasCredentials ? "enabled" : "disabled");

                _logger.LogInformation("Successfully established RTSP connection for camera {CameraName}", camera.Name);

                return Result.Ok(connectionInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error establishing RTSP connection for camera {CameraName}", camera.Name);
                return Result.Fail<CameraConnectionInfo>($"Connection failed: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DisconnectAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Disconnecting RTSP connection for camera {CameraName}", camera.Name);

                // RTSP connections are stateless, so we just log the disconnection
                await Task.Delay(100); // Simulate some cleanup work

                _logger.LogInformation("Successfully disconnected RTSP connection for camera {CameraName}", camera.Name);
                
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting RTSP connection for camera {CameraName}", camera.Name);
                return Result.Fail<bool>($"Disconnection failed: {ex.Message}");
            }
        }

        public async Task<Result<List<string>>> GetCapabilitiesAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var capabilities = new List<string>
                {
                    "rtsp_streaming",
                    "snapshot_capture",
                    "basic_authentication",
                    "tcp_transport",
                    "multiple_quality_streams"
                };

                // بررسی قابلیت‌های اضافی بر اساس پورت و نوع دوربین
                if (camera.Network.Port == 554)
                    capabilities.Add("standard_rtsp_port");

                if (camera.Network.HasCredentials)
                    capabilities.Add("digest_authentication");

                await Task.CompletedTask; // For async compliance

                return Result.Ok(capabilities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting RTSP capabilities for camera {CameraName}", camera.Name);
                return Result.Fail<List<string>>($"Failed to get capabilities: {ex.Message}");
            }
        }

        public async Task<Result<bool>> SetStreamQualityAsync(Monitoring.Domain.Aggregates.Camera.Camera camera, string quality)
        {
            try
            {
                _logger.LogInformation("Setting RTSP stream quality to {Quality} for camera {CameraName}", 
                    quality, camera.Name);

                // RTSP quality is usually managed by changing the stream path
                // This is handled in GetStreamUrlAsync method
                
                await Task.CompletedTask; // For async compliance

                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting RTSP stream quality for camera {CameraName}", camera.Name);
                return Result.Fail<bool>($"Failed to set quality: {ex.Message}");
            }
        }

        public async Task<Result<Dictionary<string, object>>> GetCameraStatusAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var status = new Dictionary<string, object>
                {
                    ["strategy"] = StrategyName,
                    ["protocol"] = "RTSP",
                    ["ip_address"] = camera.Network.IpAddress,
                    ["port"] = camera.Network.Port,
                    ["has_authentication"] = camera.Network.HasCredentials,
                    ["connection_type"] = "TCP",
                    ["supported_qualities"] = new[] { "high", "medium", "low" }
                };

                // تست اتصال سریع
                var connectionTest = await TestPingAsync(camera.Network.IpAddress);
                status["is_reachable"] = connectionTest;
                status["last_check"] = DateTime.UtcNow;

                return Result.Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting RTSP camera status for {CameraName}", camera.Name);
                return Result.Fail<Dictionary<string, object>>($"Failed to get status: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods

        private async Task<bool> TestPingAsync(string ipAddress)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ipAddress, 3000); // 3 second timeout
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TestTcpConnectionAsync(string ipAddress, int port)
        {
            try
            {
                using var tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync(ipAddress, port);
                var timeoutTask = Task.Delay(5000); // 5 second timeout

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                
                if (completedTask == connectTask && tcpClient.Connected)
                {
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TestHttpEndpointAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var httpPort = camera.Network.Port == 554 ? 80 : camera.Network.Port;
                var testUrl = $"http://{camera.Network.IpAddress}:{httpPort}/";
                
                AddAuthenticationHeaders(camera);
                
                using var response = await _httpClient.GetAsync(testUrl);
                return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateRtspUrl(Monitoring.Domain.Aggregates.Camera.Camera camera, string quality)
        {
            var network = camera.Network;
            var credentials = network.HasCredentials ? $"{network.Username}:{network.Password}@" : "";
            
            // Stream paths based on quality
            var streamPath = quality.ToLower() switch
            {
                "high" => "/stream1",
                "medium" => "/stream2", 
                "low" => "/stream3",
                _ => "/stream1"
            };

            return $"rtsp://{credentials}{network.IpAddress}:{network.Port}{streamPath}";
        }

        private string GenerateSnapshotUrl(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            var network = camera.Network;
            var httpPort = network.Port == 554 ? 80 : network.Port; // Default HTTP port if RTSP port is standard
            
            return $"http://{network.IpAddress}:{httpPort}/snapshot.jpg";
        }

        protected override async Task<Result<bool>> PerformHealthCheckAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                // مرحله 1: تست Ping
                var pingResult = await TestPingAsync(camera.Network.IpAddress);
                if (!pingResult)
                {
                    return Result.Fail<bool>("Camera is not reachable via ping");
                }

                // مرحله 2: تست اتصال TCP به پورت RTSP
                var tcpResult = await TestTcpConnectionAsync(camera.Network.IpAddress, camera.Network.Port);
                if (!tcpResult)
                {
                    return Result.Fail<bool>($"TCP connection failed on port {camera.Network.Port}");
                }

                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                LogError(ex, "HealthCheck", camera);
                return Result.Fail<bool>($"Health check failed: {ex.Message}");
            }
        }

        #endregion
    }
}

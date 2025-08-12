using FluentResults;
using Monitoring.Application.Interfaces.Camera;
using Monitoring.Common.Interfaces;
using Monitoring.Common.Utilities;
using Monitoring.Domain.Aggregates.Camera;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.Linq;
using Domain.Aggregates.Camera.ValueObjects;

namespace Monitoring.Application.Services.Camera
{
    public class CameraStreamService : ICameraStreamService
    {
        private readonly Monitoring.Infrastructure.Persistence.IUnitOfWork _unitOfWork;
        private readonly ILogger<CameraStreamService> _logger;
        private readonly HttpClient _httpClient;

        public CameraStreamService(
            Monitoring.Infrastructure.Persistence.IUnitOfWork unitOfWork, 
            ILogger<CameraStreamService> logger,
            HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<Result<string>> GetStreamUrlAsync(Guid cameraId)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.FindAsync(cameraId);
                if (camera == null)
                    return Result.Fail<string>("Camera not found");

                if (camera.Network == null)
                    return Result.Fail<string>("Camera network configuration is missing");

                // بر اساس نوع دوربین، URL مناسب را تولید می‌کنیم
                string streamUrl = camera.Type.Name.ToLower() switch
                {
                    "ip" => GenerateRtspUrl(camera),
                    "usb" => GenerateUsbStreamUrl(camera),
                    "wifi" => GenerateWifiStreamUrl(camera),
                    _ => GenerateRtspUrl(camera) // پیش‌فرض RTSP
                };

                return Result.Ok(streamUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream URL for camera {CameraId}", cameraId);
                return Result.Fail<string>($"Error getting stream URL: {ex.Message}");
            }
        }

        public async Task<Result<string>> GetSnapshotUrlAsync(Guid cameraId)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.FindAsync(cameraId);
                if (camera == null)
                    return Result.Fail<string>("Camera not found");

                if (camera.Network == null)
                    return Result.Fail<string>("Camera network configuration is missing");

                string snapshotUrl = $"http://{camera.Network.IpAddress}:{camera.Network.Port}/snapshot";
                return Result.Ok(snapshotUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting snapshot URL for camera {CameraId}", cameraId);
                return Result.Fail<string>($"Error getting snapshot URL: {ex.Message}");
            }
        }

        public async Task<Result<bool>> TestConnectionAsync(Guid cameraId)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.FindAsync(cameraId);
                if (camera == null)
                    return Result.Fail<bool>("Camera not found");

                if (camera.Network == null)
                    return Result.Fail<bool>("Camera network configuration is missing");

                // تست اتصال با ping یا HTTP request
                bool isConnected = await PingCameraAsync(camera);
                
                if (isConnected)
                {
                    // بروزرسانی وضعیت دوربین
                    camera.SetStatus(CameraStatus.Active);
                    await _unitOfWork.CameraRepository.UpdateAsync(camera);
                    await _unitOfWork.SaveAsync();
                }
                else
                {
                    camera.SetStatus(CameraStatus.Inactive);
                    await _unitOfWork.CameraRepository.UpdateAsync(camera);
                    await _unitOfWork.SaveAsync();
                }

                return Result.Ok(isConnected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection for camera {CameraId}", cameraId);
                return Result.Fail<bool>($"Error testing connection: {ex.Message}");
            }
        }

        public async Task<Result<byte[]>> CaptureSnapshotAsync(Guid cameraId)
        {
            try
            {
                var snapshotUrlResult = await GetSnapshotUrlAsync(cameraId);
                if (snapshotUrlResult.IsFailed)
                    return Result.Fail<byte[]>(snapshotUrlResult.Errors.FirstOrDefault()?.Message ?? "Failed to get snapshot URL");

                var camera = await _unitOfWork.CameraRepository.FindAsync(cameraId);
                if (camera?.Network != null && !string.IsNullOrEmpty(camera.Network.Username))
                {
                    // اضافه کردن authentication
                    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{camera.Network.Username}:{camera.Network.Password}"));
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                }

                var response = await _httpClient.GetAsync(snapshotUrlResult.Value);
                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    return Result.Ok(imageBytes);
                }
                else
                {
                    return Result.Fail<byte[]>($"Failed to capture snapshot. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing snapshot for camera {CameraId}", cameraId);
                return Result.Fail<byte[]>($"Error capturing snapshot: {ex.Message}");
            }
        }

        public async Task<Result<string>> GetRtspStreamUrlAsync(Guid cameraId)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.FindAsync(cameraId);
                if (camera == null)
                    return Result.Fail<string>("Camera not found");

                string rtspUrl = GenerateRtspUrl(camera);
                return Result.Ok(rtspUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting RTSP URL for camera {CameraId}", cameraId);
                return Result.Fail<string>($"Error getting RTSP URL: {ex.Message}");
            }
        }

        public async Task<Result<string>> GetHttpStreamUrlAsync(Guid cameraId)
        {
            try
            {
                var camera = await _unitOfWork.CameraRepository.FindAsync(cameraId);
                if (camera == null)
                    return Result.Fail<string>("Camera not found");

                if (camera.Network == null)
                    return Result.Fail<string>("Camera network configuration is missing");

                string httpUrl = $"http://{camera.Network.IpAddress}:{camera.Network.Port}/video";
                return Result.Ok(httpUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting HTTP stream URL for camera {CameraId}", cameraId);
                return Result.Fail<string>($"Error getting HTTP stream URL: {ex.Message}");
            }
        }

        #region Private Methods

        private string GenerateRtspUrl(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            if (camera.Network == null) return string.Empty;

            string credentials = string.Empty;
            if (!string.IsNullOrEmpty(camera.Network.Username))
            {
                credentials = $"{camera.Network.Username}:{camera.Network.Password}@";
            }

            return $"rtsp://{credentials}{camera.Network.IpAddress}:{camera.Network.Port}/stream";
        }

        private string GenerateUsbStreamUrl(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            // برای دوربین‌های USB معمولاً از device path استفاده می‌شود
            return $"/dev/video0"; // یا device path مناسب
        }

        private string GenerateWifiStreamUrl(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            if (camera.Network == null) return string.Empty;
            return $"http://{camera.Network.IpAddress}:{camera.Network.Port}/stream";
        }

        private async Task<bool> PingCameraAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                if (camera.Network == null) return false;

                // تست اتصال با HTTP HEAD request
                var testUrl = $"http://{camera.Network.IpAddress}:{camera.Network.Port}";
                
                if (!string.IsNullOrEmpty(camera.Network.Username))
                {
                    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{camera.Network.Username}:{camera.Network.Password}"));
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                }

                _httpClient.Timeout = TimeSpan.FromSeconds(5); // timeout 5 ثانیه
                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, testUrl));
                
                return response.StatusCode == HttpStatusCode.OK || 
                       response.StatusCode == HttpStatusCode.Unauthorized; // حتی 401 نشان‌دهنده اتصال است
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ping failed for camera {CameraId}", camera.Id);
                return false;
            }
        }

        #endregion
    }
}

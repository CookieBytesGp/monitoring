 using App.Services.Interfaces;
using Microsoft.Extensions.Options;
using App.Models.Settings;

namespace App.Services.BackgroundServices
{
    public class CameraHealthCheckService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CameraHealthCheckService> _logger;
        private readonly IConfiguration _configuration;

        public CameraHealthCheckService(
            IServiceProvider services,
            ILogger<CameraHealthCheckService> logger,
            IConfiguration configuration)
        {
            _services = services;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformHealthCheck();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking cameras health");
                }

                var interval = _configuration.GetValue<int>("Settings:CameraRefreshInterval", 5);
                await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
            }
        }

        private async Task PerformHealthCheck()
        {
            using var scope = _services.CreateScope();
            var cameraService = scope.ServiceProvider.GetRequiredService<ICameraService>();

            var cameras = await cameraService.GetAllCamerasAsync();
            foreach (var camera in cameras)
            {
                try
                {
                    var isActive = await cameraService.TestCameraConnectionAsync(camera.Id);
                    _logger.LogInformation($"Camera {camera.Name} health check: {(isActive ? "Active" : "Inactive")}");

                    if (isActive && _configuration.GetValue<bool>("Settings:EnableMotionDetection", true))
                    {
                        await CheckForMotion(camera.Id, cameraService);
                    }
                    else if (!isActive && _configuration.GetValue<bool>("Settings:AutoReconnectDevices", true))
                    {
                        _logger.LogInformation($"Attempting to reconnect camera {camera.Name}");
                        // Implement reconnection logic here
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error checking camera {camera.Name} health");
                }
            }
        }

        private async Task CheckForMotion(int cameraId, ICameraService cameraService)
        {
            try
            {
                // Get current frame
                var currentFrame = await cameraService.GetCameraSnapshotAsync(cameraId);
                
                // In a real implementation, you would:
                // 1. Compare with previous frame
                // 2. Use motion detection algorithm
                // 3. Trigger notifications if motion detected
                // 4. Store motion events in database

                // For now, we'll just log that we checked
                _logger.LogDebug($"Motion check performed for camera {cameraId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error performing motion detection for camera {cameraId}");
            }
        }
    }
}
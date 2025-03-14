 using App.Services.Interfaces;
using Microsoft.Extensions.Options;
using App.Models.Settings;

namespace App.Services.BackgroundServices
{
    public class MonitorHealthCheckService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<MonitorHealthCheckService> _logger;
        private readonly IConfiguration _configuration;

        public MonitorHealthCheckService(
            IServiceProvider services,
            ILogger<MonitorHealthCheckService> logger,
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
                    _logger.LogError(ex, "Error occurred while checking monitors health");
                }

                var interval = _configuration.GetValue<int>("Settings:MonitorRefreshInterval", 30);
                await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
            }
        }

        private async Task PerformHealthCheck()
        {
            using var scope = _services.CreateScope();
            var monitorService = scope.ServiceProvider.GetRequiredService<IMonitorService>();

            var monitors = await monitorService.GetAllMonitorsAsync();
            foreach (var monitor in monitors)
            {
                try
                {
                    var isActive = await monitorService.PingMonitorAsync(monitor.Id);
                    _logger.LogInformation($"Monitor {monitor.Name} health check: {(isActive ? "Active" : "Inactive")}");

                    if (!isActive && _configuration.GetValue<bool>("Settings:AutoReconnectDevices", true))
                    {
                        _logger.LogInformation($"Attempting to reconnect monitor {monitor.Name}");
                        // Implement reconnection logic here
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error checking monitor {monitor.Name} health");
                }
            }
        }
    }
}
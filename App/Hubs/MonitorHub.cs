 using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace App.Hubs
{
    [Authorize]
    public class MonitorHub : Hub
    {
        private readonly ILogger<MonitorHub> _logger;

        public MonitorHub(ILogger<MonitorHub> logger)
        {
            _logger = logger;
        }

        public async Task UpdateMonitorStatus(int monitorId, bool isActive)
        {
            await Clients.All.SendAsync("UpdateMonitorStatus", monitorId, isActive);
            _logger.LogInformation($"Monitor {monitorId} status updated to {isActive}");
        }

        public async Task UpdateMonitorContent(int monitorId, string content)
        {
            await Clients.All.SendAsync("UpdateMonitorContent", monitorId, content);
            _logger.LogInformation($"Monitor {monitorId} content updated");
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
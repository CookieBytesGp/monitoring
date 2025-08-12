using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace App.Hubs
{
    [Authorize]
    public class CameraHub : Hub
    {
        private readonly ILogger<CameraHub> _logger;

        public CameraHub(ILogger<CameraHub> logger)
        {
            _logger = logger;
        }

        public async Task UpdateCameraStatus(int cameraId, bool isActive)
        {
            await Clients.All.SendAsync("UpdateCameraStatus", cameraId, isActive);
            _logger.LogInformation($"Camera {cameraId} status updated to {isActive}");
        }

        public async Task NotifyMotionDetected(int cameraId, string timestamp)
        {
            await Clients.All.SendAsync("MotionDetected", cameraId, timestamp);
            _logger.LogInformation($"Motion detected on camera {cameraId} at {timestamp}");
        }

        public async Task JoinCameraGroup(int cameraId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Camera_{cameraId}");
            _logger.LogInformation($"Client {Context.ConnectionId} joined Camera_{cameraId} group");
        }

        public async Task LeaveCameraGroup(int cameraId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Camera_{cameraId}");
            _logger.LogInformation($"Client {Context.ConnectionId} left Camera_{cameraId} group");
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


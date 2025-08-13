using Monitoring.Application.Interfaces.Realtime;

namespace Monitoring.ApiGateway.Services;

public class ApiCameraNotifications : ICameraNotifications
{
    public Task UpdateCameraStatusAsync(Guid cameraId, bool isActive)
    {
        // In API Gateway, we don't need real-time notifications
        // This could be extended to send events to a message queue or external service
        Console.WriteLine($"Camera {cameraId} status updated to {isActive}");
        return Task.CompletedTask;
    }

    public Task NotifyMotionDetectedAsync(Guid cameraId, string timestamp)
    {
        // In API Gateway, we don't need real-time notifications
        // This could be extended to send events to a message queue or external service
        Console.WriteLine($"Motion detected on camera {cameraId} at {timestamp}");
        return Task.CompletedTask;
    }
}

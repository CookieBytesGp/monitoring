using System.Threading.Tasks;

namespace Monitoring.Application.Interfaces.Realtime;

public interface ICameraNotifications
{
    Task UpdateCameraStatusAsync(Guid cameraId, bool isActive);
    Task NotifyMotionDetectedAsync(Guid cameraId, string timestamp);
}




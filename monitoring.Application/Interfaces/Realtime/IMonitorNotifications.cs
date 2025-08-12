using System.Threading.Tasks;

namespace Monitoring.Application.Interfaces.Realtime;

public interface IMonitorNotifications
{
    Task UpdateMonitorStatusAsync(int monitorId, bool isActive);
    Task UpdateMonitorContentAsync(int monitorId, string content);
}




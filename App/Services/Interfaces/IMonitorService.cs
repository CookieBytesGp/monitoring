 using App.Models.Monitor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.Interfaces
{
    public interface IMonitorService
    {
        Task<IEnumerable<MonitorDevice>> GetAllMonitorsAsync();
        Task<MonitorDevice> GetMonitorByIdAsync(int id);
        Task<MonitorDevice> CreateMonitorAsync(MonitorDevice monitor);
        Task<MonitorDevice> UpdateMonitorAsync(MonitorDevice monitor);
        Task<bool> DeleteMonitorAsync(int id);
        Task<bool> UpdateMonitorStatusAsync(int id, bool isActive);
        Task<bool> UpdateMonitorContentAsync(int id, string content);
        Task<bool> PingMonitorAsync(int id);
    }
}
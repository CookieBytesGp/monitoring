 using App.Services.Interfaces;
using App.Models.Monitor;
using App.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Data;

namespace App.Services
{
    public class MonitorService : IMonitorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<MonitorHub> _monitorHub;
        private readonly ILogger<MonitorService> _logger;

        public MonitorService(
            ApplicationDbContext context,
            IHubContext<MonitorHub> monitorHub,
            ILogger<MonitorService> logger)
        {
            _context = context;
            _monitorHub = monitorHub;
            _logger = logger;
        }

        public async Task<IEnumerable<MonitorDevice>> GetAllMonitorsAsync()
        {
            return await _context.Monitors
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<MonitorDevice> GetMonitorByIdAsync(int id)
        {
            return await _context.Monitors.FindAsync(id);
        }

        public async Task<MonitorDevice> CreateMonitorAsync(MonitorDevice monitor)
        {
            _context.Monitors.Add(monitor);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Monitor created: {monitor.Name}");
            return monitor;
        }

        public async Task<MonitorDevice> UpdateMonitorAsync(MonitorDevice monitor)
        {
            _context.Entry(monitor).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Monitor updated: {monitor.Name}");
            return monitor;
        }

        public async Task<bool> DeleteMonitorAsync(int id)
        {
            var monitor = await _context.Monitors.FindAsync(id);
            if (monitor == null)
                return false;

            _context.Monitors.Remove(monitor);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Monitor deleted: {monitor.Name}");
            return true;
        }

        public async Task<bool> UpdateMonitorStatusAsync(int id, bool isActive)
        {
            var monitor = await _context.Monitors.FindAsync(id);
            if (monitor == null)
                return false;

            monitor.IsActive = isActive;
            monitor.LastPing = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _monitorHub.Clients.All.SendAsync("UpdateMonitorStatus", id, isActive);
            _logger.LogInformation($"Monitor {id} status updated to {isActive}");
            return true;
        }

        public async Task<bool> UpdateMonitorContentAsync(int id, string content)
        {
            var monitor = await _context.Monitors.FindAsync(id);
            if (monitor == null)
                return false;

            monitor.CurrentContent = content;
            await _context.SaveChangesAsync();

            await _monitorHub.Clients.All.SendAsync("UpdateMonitorContent", id, content);
            _logger.LogInformation($"Monitor {id} content updated");
            return true;
        }

        public async Task<bool> PingMonitorAsync(int id)
        {
            var monitor = await _context.Monitors.FindAsync(id);
            if (monitor == null)
                return false;

            try
            {
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = await ping.SendPingAsync(monitor.IpAddress);
                var isActive = reply.Status == System.Net.NetworkInformation.IPStatus.Success;

                await UpdateMonitorStatusAsync(id, isActive);
                return isActive;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error pinging monitor {id}: {ex.Message}");
                await UpdateMonitorStatusAsync(id, false);
                return false;
            }
        }
    }
}
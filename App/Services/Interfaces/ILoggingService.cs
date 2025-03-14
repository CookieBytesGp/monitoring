using App.Models;

namespace App.Services.Interfaces
{
    public interface ILoggingService
    {
        Task LogDeviceEventAsync(string deviceType, string deviceId, string eventType, string details);
        Task LogUserActivityAsync(string userId, string action, string details);
        Task LogSystemEventAsync(string eventType, string message, string severity = "Information");
        Task LogSecurityEventAsync(string eventType, string userId, string details, string ipAddress);
        Task LogErrorAsync(Exception ex, string context, string additionalInfo = null);
        Task<IEnumerable<LogEntry>> GetRecentLogsAsync(string type, int count = 100);
        Task<IEnumerable<LogEntry>> GetLogsByDateRangeAsync(DateTime start, DateTime end, string type = null);
    }
}
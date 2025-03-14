using App.Data;
using App.Models;
using App.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace App.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoggingService> _logger;
        private readonly IEmailService _emailService;

        public LoggingService(
            ApplicationDbContext context,
            ILogger<LoggingService> logger,
            IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task LogDeviceEventAsync(string deviceType, string deviceId, string eventType, string details)
        {
            var log = new SystemLog
            {
                EventType = eventType,
                Category = "Device",
                Source = deviceType,
                DeviceId = deviceId,
                Severity = "Information",
                Message = details
            };

            await SaveLogAsync(log);
        }

        public async Task LogUserActivityAsync(string userId, string action, string details)
        {
            var log = new SystemLog
            {
                EventType = "UserActivity",
                Category = "User",
                UserId = userId,
                Severity = "Information",
                Message = details,
                AdditionalData = JsonSerializer.Serialize(new { Action = action })
            };

            await SaveLogAsync(log);
        }

        public async Task LogSystemEventAsync(string eventType, string message, string severity = "Information")
        {
            var log = new SystemLog
            {
                EventType = eventType,
                Category = "System",
                Severity = severity,
                Message = message
            };

            await SaveLogAsync(log);

            // Send email notification for critical system events
            if (severity.Equals("Critical", StringComparison.OrdinalIgnoreCase))
            {
                await _emailService.SendSystemAlertAsync(eventType, message, severity);
            }
        }

        public async Task LogSecurityEventAsync(string eventType, string userId, string details, string ipAddress)
        {
            var log = new SystemLog
            {
                EventType = eventType,
                Category = "Security",
                UserId = userId,
                IpAddress = ipAddress,
                Severity = "Warning",
                Message = details
            };

            await SaveLogAsync(log);
        }

        public async Task LogErrorAsync(Exception ex, string context, string additionalInfo = null)
        {
            var log = new SystemLog
            {
                EventType = "Error",
                Category = "Error",
                Source = context,
                Severity = "Error",
                Message = ex.Message,
                AdditionalData = JsonSerializer.Serialize(new
                {
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message,
                    AdditionalInfo = additionalInfo
                })
            };

            await SaveLogAsync(log);

            // Send email notification for errors
            await _emailService.SendSystemAlertAsync(
                "System Error",
                $"Error in {context}: {ex.Message}",
                "Error");
        }

        public async Task<IEnumerable<LogEntry>> GetRecentLogsAsync(string type, int count = 100)
        {
            var query = _context.SystemLogs.AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(l => l.Category == type || l.EventType == type);
            }

            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .Select(l => new 
                {
                    l.Timestamp,
                    l.EventType,
                    l.Category,
                    l.Source,
                    l.Severity,
                    l.Message,
                    l.UserId,
                    l.DeviceId,
                    l.IpAddress,
                    l.AdditionalData
                })
                .ToListAsync();

            return logs.Select(l => new LogEntry
            {
                Timestamp = l.Timestamp,
                EventType = l.EventType,
                Category = l.Category,
                Source = l.Source,
                Severity = l.Severity,
                Message = l.Message,
                UserId = l.UserId,
                DeviceId = l.DeviceId,
                IpAddress = l.IpAddress,
                AdditionalData = !string.IsNullOrEmpty(l.AdditionalData)
                    ? JsonDocument.Parse(l.AdditionalData).RootElement
                    : null
            });
        }

        public async Task<IEnumerable<LogEntry>> GetLogsByDateRangeAsync(DateTime start, DateTime end, string type = null)
        {
            var query = _context.SystemLogs
                .Where(l => l.Timestamp >= start && l.Timestamp <= end);

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(l => l.Category == type || l.EventType == type);
            }

            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new
                {
                    l.Timestamp,
                    l.EventType,
                    l.Category,
                    l.Source,
                    l.Severity,
                    l.Message,
                    l.UserId,
                    l.DeviceId,
                    l.IpAddress,
                    l.AdditionalData
                })
                .ToListAsync();

            return logs.Select(l => new LogEntry
            {
                Timestamp = l.Timestamp,
                EventType = l.EventType,
                Category = l.Category,
                Source = l.Source,
                Severity = l.Severity,
                Message = l.Message,
                UserId = l.UserId,
                DeviceId = l.DeviceId,
                IpAddress = l.IpAddress,
                AdditionalData = !string.IsNullOrEmpty(l.AdditionalData)
                    ? JsonDocument.Parse(l.AdditionalData).RootElement
                    : null
            });
        }

        private async Task SaveLogAsync(SystemLog log)
        {
            try
            {
                _context.SystemLogs.Add(log);
                await _context.SaveChangesAsync();

                // Also log to the built-in logging system
                var logLevel = log.Severity.ToLower() switch
                {
                    "error" => LogLevel.Error,
                    "warning" => LogLevel.Warning,
                    "critical" => LogLevel.Critical,
                    _ => LogLevel.Information
                };

                _logger.Log(logLevel, "{Category} - {EventType}: {Message}", 
                    log.Category, log.EventType, log.Message);
            }
            catch (Exception ex)
            {
                // If database logging fails, at least log to the built-in logging system
                _logger.LogError(ex, "Failed to save log entry to database: {Message}", ex.Message);
            }
        }
    }
}
using App.Data;
using App.Models;
using App.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Services
{
    public class MotionAnalyticsService : IMotionAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICameraService _cameraService;
        private readonly ILoggingService _loggingService;
        private readonly ILogger<MotionAnalyticsService> _logger;

        public MotionAnalyticsService(
            ApplicationDbContext context,
            ICameraService cameraService,
            ILoggingService loggingService,
            ILogger<MotionAnalyticsService> logger)
        {
            _context = context;
            _cameraService = cameraService;
            _loggingService = loggingService;
            _logger = logger;
        }

        public async Task<MotionEvent> RecordMotionEventAsync(string cameraId, double motionPercentage, string imagePath)
        {
            if (!int.TryParse(cameraId, out int cameraIdInt))
            {
                throw new ArgumentException("Invalid camera ID format", nameof(cameraId));
            }
            var camera = await _cameraService.GetCameraByIdAsync(cameraIdInt);
            if (camera == null)
            {
                throw new ArgumentException("Camera not found", nameof(cameraId));
            }

            var motionEvent = new MotionEvent
            {
                CameraId = cameraIdInt,
                CameraName = camera.Name,
                Location = camera.Location,
                MotionPercentage = (decimal)motionPercentage,
                ImagePath = imagePath,
                Timestamp = DateTime.UtcNow
            };

            _context.MotionEvents.Add(motionEvent);
            await _context.SaveChangesAsync();

            await _loggingService.LogDeviceEventAsync(
                "Camera",
                cameraId,
                "MotionDetected",
                $"Motion event recorded with {motionPercentage:P2} coverage");

            return motionEvent;
        }

        public async Task<MotionEvent> GetEventByIdAsync(int eventId)
        {
            return await _context.MotionEvents.FindAsync(eventId);
        }

        public async Task<IEnumerable<MotionEvent>> GetEventsByCameraAsync(
            string cameraId, 
            DateTime? start = null, 
            DateTime? end = null)
        {
            if (!int.TryParse(cameraId, out int cameraIdInt))
            {
                throw new ArgumentException("Invalid camera ID format", nameof(cameraId));
            }

            var query = _context.MotionEvents
                .Where(e => e.CameraId == cameraIdInt);

            if (start.HasValue)
                query = query.Where(e => e.Timestamp >= start.Value);

            if (end.HasValue)
                query = query.Where(e => e.Timestamp <= end.Value);

            return await query
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<MotionEvent>> GetRecentEventsAsync(int count = 100)
        {
            return await _context.MotionEvents
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetEventCountByCamera(DateTime start, DateTime end)
        {
            return await _context.MotionEvents
                .Where(e => e.Timestamp >= start && e.Timestamp <= end)
                .GroupBy(e => e.CameraId)
                .ToDictionaryAsync(
                    g => g.Key.ToString(),
                    g => g.Count()
                );
        }

        public async Task<Dictionary<int, int>> GetEventsByHourAsync(string cameraId, DateTime date)
        {
            if (!int.TryParse(cameraId, out int cameraIdInt))
            {
                throw new ArgumentException("Invalid camera ID format", nameof(cameraId));
            }

            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var events = await _context.MotionEvents
                .Where(e => e.CameraId == cameraIdInt &&
                           e.Timestamp >= startDate &&
                           e.Timestamp < endDate)
                .ToListAsync();

            return events
                .GroupBy(e => e.Timestamp.Hour)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );
        }

        public async Task<Dictionary<DateTime, int>> GetEventsByDayAsync(
            string cameraId, 
            DateTime start, 
            DateTime end)
        {
            if (!int.TryParse(cameraId, out int cameraIdInt))
            {
                throw new ArgumentException("Invalid camera ID format", nameof(cameraId));
            }

            var events = await _context.MotionEvents
                .Where(e => e.CameraId == cameraIdInt &&
                           e.Timestamp.Date >= start.Date &&
                           e.Timestamp.Date <= end.Date)
                .ToListAsync();

            return events
                .GroupBy(e => e.Timestamp.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );
        }

        public async Task AcknowledgeEventAsync(int eventId, string userId)
        {
            var motionEvent = await _context.MotionEvents.FindAsync(eventId);
            if (motionEvent == null)
            {
                throw new ArgumentException("Event not found", nameof(eventId));
            }

            motionEvent.Acknowledged = true;
            motionEvent.AcknowledgedAt = DateTime.UtcNow;
            motionEvent.AcknowledgedBy = userId;

            await _context.SaveChangesAsync();
        }

        public async Task<MotionAnalytics> GetAnalyticsAsync(string cameraId, DateTime start, DateTime end)
        {
            if (!int.TryParse(cameraId, out int cameraIdInt))
            {
                throw new ArgumentException("Invalid camera ID format", nameof(cameraId));
            }

            var events = await _context.MotionEvents
                .Where(e => e.CameraId == cameraIdInt &&
                           e.Timestamp >= start &&
                           e.Timestamp <= end)
                .ToListAsync();

            if (!events.Any())
            {
                return new MotionAnalytics
                {
                    EventsByHour = new Dictionary<int, int>(),
                    EventsByLocation = new Dictionary<string, int>()
                };
            }

            var analytics = new MotionAnalytics
            {
                TotalEvents = events.Count,
                AverageMotionPercentage = events.Average(e => (double)e.MotionPercentage),
                UnacknowledgedEvents = events.Count(e => !e.Acknowledged),
                PeakMotionPercentage = events.Max(e => (double)e.MotionPercentage),
                EventsByHour = events
                    .GroupBy(e => e.Timestamp.Hour)
                    .ToDictionary(g => g.Key, g => g.Count()),
                EventsByLocation = events
                    .GroupBy(e => e.Location)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            // Find peak activity time
            var peakHour = analytics.EventsByHour
                .OrderByDescending(kvp => kvp.Value)
                .First().Key;
            analytics.PeakActivityTime = start.Date.AddHours(peakHour);

            return analytics;
        }

        public async Task DeleteEventAsync(int eventId)
        {
            var motionEvent = await _context.MotionEvents.FindAsync(eventId);
            if (motionEvent != null)
            {
                _context.MotionEvents.Remove(motionEvent);
                await _context.SaveChangesAsync();

                // Delete associated image if exists
                if (!string.IsNullOrEmpty(motionEvent.ImagePath) && 
                    File.Exists(motionEvent.ImagePath))
                {
                    File.Delete(motionEvent.ImagePath);
                }
            }
        }

        public async Task DeleteEventsBeforeAsync(DateTime date)
        {
            var eventsToDelete = await _context.MotionEvents
                .Where(e => e.Timestamp < date)
                .ToListAsync();

            foreach (var evt in eventsToDelete)
            {
                if (!string.IsNullOrEmpty(evt.ImagePath) && 
                    File.Exists(evt.ImagePath))
                {
                    File.Delete(evt.ImagePath);
                }
            }

            _context.MotionEvents.RemoveRange(eventsToDelete);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MotionEvent>> GetMotionEventsAsync(DateTime startDate, DateTime endDate, string cameraId = null)
        {
            try
            {
                var query = _context.MotionEvents
                    .Include(e => e.Camera)
                    .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate);

                if (!string.IsNullOrEmpty(cameraId) && int.TryParse(cameraId, out int camId))
                {
                    query = query.Where(e => e.CameraId == camId);
                }

                return await query
                    .OrderByDescending(e => e.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving motion events for date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<int> GetMotionEventCountAsync(string cameraId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var query = _context.MotionEvents
                    .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate);

                if (!string.IsNullOrEmpty(cameraId))
                {
                    if (!int.TryParse(cameraId, out int cameraIdInt))
                    {
                        throw new ArgumentException("Invalid camera ID format", nameof(cameraId));
                    }
                    query = query.Where(e => e.CameraId == cameraIdInt);
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting motion event count for camera {CameraId} from {StartDate} to {EndDate}", 
                    cameraId, startDate, endDate);
                throw;
            }
        }

        public async Task<MotionStats> GetMotionStatsAsync(int eventId)
        {
            try
            {
                var motionEvent = await _context.MotionEvents
                    .Include(e => e.Camera)
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (motionEvent == null)
                {
                    throw new ArgumentException("Motion event not found", nameof(eventId));
                }

                // Calculate motion statistics
                var stats = new MotionStats
                {
                    AverageMotion = (double)motionEvent.MotionPercentage,
                    PeakMotion = (double)motionEvent.MotionPercentage,
                    Duration = TimeSpan.FromSeconds(1), // Default duration
                    MotionArea = new App.Services.Interfaces.Rectangle
                    {
                        X = motionEvent.DetectionRegion.X,
                        Y = motionEvent.DetectionRegion.Y,
                        Width = motionEvent.DetectionRegion.Width,
                        Height = motionEvent.DetectionRegion.Height
                    },
                    MotionPoints = new List<App.Services.Interfaces.Point>(), // Add actual motion points if available
                    PixelsChanged = (int)(motionEvent.MotionPercentage * 
                        (motionEvent.DetectionRegion.Width * motionEvent.DetectionRegion.Height) / 100)
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting motion stats for event {EventId}", eventId);
                throw;
            }
        }
    }
}
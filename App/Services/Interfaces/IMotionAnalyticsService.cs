using App.Models;
using SixLabors.ImageSharp;

namespace App.Services.Interfaces
{
    public interface IMotionAnalyticsService
    {
        Task<MotionEvent> RecordMotionEventAsync(string cameraId, double motionPercentage, string imagePath);
        Task<MotionEvent> GetEventByIdAsync(int eventId);
        Task<IEnumerable<MotionEvent>> GetEventsByCameraAsync(string cameraId, DateTime? start = null, DateTime? end = null);
        Task<IEnumerable<MotionEvent>> GetRecentEventsAsync(int count = 100);
        Task<Dictionary<string, int>> GetEventCountByCamera(DateTime start, DateTime end);
        Task<Dictionary<int, int>> GetEventsByHourAsync(string cameraId, DateTime date);
        Task<Dictionary<DateTime, int>> GetEventsByDayAsync(string cameraId, DateTime start, DateTime end);
        Task AcknowledgeEventAsync(int eventId, string userId);
        Task<MotionAnalytics> GetAnalyticsAsync(string cameraId, DateTime start, DateTime end);
        Task DeleteEventAsync(int eventId);
        Task DeleteEventsBeforeAsync(DateTime date);
        Task<List<MotionEvent>> GetMotionEventsAsync(DateTime startDate, DateTime endDate, string cameraId = null);
        Task<int> GetMotionEventCountAsync(string cameraId, DateTime startDate, DateTime endDate);
        Task<MotionStats> GetMotionStatsAsync(int eventId);
    }

    public class MotionAnalytics
    {
        public int TotalEvents { get; set; }
        public double AverageMotionPercentage { get; set; }
        public Dictionary<int, int> EventsByHour { get; set; }
        public int UnacknowledgedEvents { get; set; }
        public DateTime PeakActivityTime { get; set; }
        public double PeakMotionPercentage { get; set; }
        public TimeSpan AverageEventDuration { get; set; }
        public Dictionary<string, int> EventsByLocation { get; set; }
    }

    public class MotionStats
    {
        public double AverageMotion { get; set; }
        public double PeakMotion { get; set; }
        public TimeSpan Duration { get; set; }
        public int PixelsChanged { get; set; }
        public Rectangle MotionArea { get; set; }
        public List<Point> MotionPoints { get; set; }
    }
}
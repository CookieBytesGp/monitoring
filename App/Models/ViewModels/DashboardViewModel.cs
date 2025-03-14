using System;
using System.Collections.Generic;

namespace App.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Camera Statistics
        public int TotalCameras { get; set; }
        public int ActiveCameras { get; set; }
        public int InactiveCameras { get; set; }

        // Motion Events
        public int TodayEvents { get; set; }
        public int YesterdayEvents { get; set; }
        public int UnacknowledgedEvents { get; set; }

        // Event Distribution
        public List<int> HourlyEventDistribution { get; set; }

        // Lists
        public List<RecentEventViewModel> RecentEvents { get; set; }
        public List<ActiveCameraViewModel> ActiveCamerasList { get; set; }

        // System Health
        public SystemHealthViewModel SystemHealth { get; set; }
    }

    public class RecentEventViewModel
    {
        public int Id { get; set; }
        public string CameraName { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal MotionPercentage { get; set; }
        public string Location { get; set; }
        public bool IsAcknowledged { get; set; }
        public bool HasImage { get; set; }
    }

    public class ActiveCameraViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public DateTime LastActive { get; set; }
        public string StreamUrl { get; set; }
    }

    public class SystemHealthViewModel
    {
        public SystemErrorViewModel LastError { get; set; }
        public List<SystemAlertViewModel> RecentAlerts { get; set; }
    }

    public class SystemErrorViewModel
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
    }

    public class SystemAlertViewModel
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; }
    }
} 
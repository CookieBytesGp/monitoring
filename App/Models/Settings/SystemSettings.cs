 using System.ComponentModel.DataAnnotations;

namespace App.Models.Settings
{
    public class SystemSettings
    {
        [Required]
        [Display(Name = "Monitor Refresh Interval (seconds)")]
        [Range(5, 3600)]
        public int MonitorRefreshInterval { get; set; } = 30;

        [Required]
        [Display(Name = "Camera Refresh Interval (seconds)")]
        [Range(1, 3600)]
        public int CameraRefreshInterval { get; set; } = 5;

        [Required]
        [Display(Name = "Default Theme")]
        public string DefaultTheme { get; set; } = "light";

        [Required]
        [Display(Name = "Enable Motion Detection")]
        public bool EnableMotionDetection { get; set; } = true;

        [Required]
        [Display(Name = "Motion Detection Sensitivity")]
        [Range(1, 10)]
        public int MotionDetectionSensitivity { get; set; } = 5;

        [Required]
        [Display(Name = "Enable Email Notifications")]
        public bool EnableEmailNotifications { get; set; } = false;

        [EmailAddress]
        [Display(Name = "Notification Email")]
        public string NotificationEmail { get; set; }

        [Required]
        [Display(Name = "Auto-Reconnect Devices")]
        public bool AutoReconnectDevices { get; set; } = true;

        [Required]
        [Display(Name = "Log Retention Days")]
        [Range(1, 365)]
        public int LogRetentionDays { get; set; } = 30;

        [Required]
        [Display(Name = "Maximum Concurrent Streams")]
        [Range(1, 100)]
        public int MaxConcurrentStreams { get; set; } = 10;
    }
}
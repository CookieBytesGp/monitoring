using System;

namespace App.Models.Monitor
{
    public class MonitorDevice
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string IpAddress { get; set; }
        public string DisplayResolution { get; set; }
        public string CurrentContent { get; set; }
        public DateTime LastPing { get; set; }
        public bool IsActive { get; set; }
        public bool IsConnected { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }

        public MonitorDevice()
        {
            LastPing = DateTime.UtcNow;
            IsActive = true;
            IsConnected = false;
            Status = "Initialized";
        }
    }
} 
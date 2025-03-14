using System;
using System.Text.Json;

namespace App.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }
        public string Category { get; set; }
        public string Source { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public string DeviceId { get; set; }
        public string IpAddress { get; set; }
        public JsonElement? AdditionalData { get; set; }
    }
} 
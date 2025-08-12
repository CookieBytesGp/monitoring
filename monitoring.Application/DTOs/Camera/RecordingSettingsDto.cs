using System;

namespace Monitoring.Application.DTOs.Camera
{
    public class RecordingSettingsDto
    {
        public bool IsEnabled { get; set; }
        public string Quality { get; set; }
        public TimeSpan Duration { get; set; }
        public string StoragePath { get; set; }
    }
}

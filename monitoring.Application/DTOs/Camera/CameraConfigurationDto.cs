using System;

namespace Monitoring.Application.DTOs.Camera
{
    public class CameraConfigurationDto
    {
        public string Resolution { get; set; }
        public int FrameRate { get; set; }
        public string VideoCodec { get; set; }
        public int Bitrate { get; set; }
        public bool AudioEnabled { get; set; }
        public string AudioCodec { get; set; }
        public MotionDetectionSettingsDto MotionDetection { get; set; }
        public RecordingSettingsDto Recording { get; set; }
    }
}

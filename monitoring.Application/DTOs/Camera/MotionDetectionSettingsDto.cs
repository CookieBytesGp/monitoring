namespace Monitoring.Application.DTOs.Camera
{
    public class MotionDetectionSettingsDto
    {
        public bool IsEnabled { get; set; }
        public int Sensitivity { get; set; }
        public string DetectionZone { get; set; }
    }
}

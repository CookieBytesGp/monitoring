namespace App.Models
{
    public class MotionDetectionSettings
    {
        public string ImageSavePath { get; set; }
        public double DefaultSensitivity { get; set; }
        public int MinimumMotionFrames { get; set; }
        public double ThresholdValue { get; set; }
        public bool SaveImages { get; set; }
    }
} 
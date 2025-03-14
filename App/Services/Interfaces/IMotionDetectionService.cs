 namespace App.Services.Interfaces
{
    public interface IMotionDetectionService
    {
        Task StartDetectionAsync(string cameraId);
        Task StopDetectionAsync(string cameraId);
        Task<bool> IsDetectionActiveAsync(string cameraId);
        Task UpdateSensitivityAsync(string cameraId, double sensitivity);
        Task UpdateRegionOfInterestAsync(string cameraId, Rectangle roi);
        Task<MotionDetectionSettings> GetSettingsAsync(string cameraId);
    }

    public class Rectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class MotionDetectionSettings
    {
        public bool IsActive { get; set; }
        public double Sensitivity { get; set; }
        public Rectangle RegionOfInterest { get; set; }
        public int MinimumMotionFrames { get; set; }
        public double ThresholdValue { get; set; }
        public bool SaveDetectionImages { get; set; }
        public string SavePath { get; set; }
    }
}
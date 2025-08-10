using App.Models.Camera;
using System.ComponentModel.DataAnnotations;

namespace App.Models.ViewModels
{
    // Main Image Management View Model
    public class ImageManagementViewModel
    {
        public List<CameraImageViewModel> Cameras { get; set; } = new();
        public int TotalCameras => Cameras.Count;
        public int ActiveCameras => Cameras.Count(c => c.IsActive);
        public int InactiveCameras => Cameras.Count(c => !c.IsActive);
    }

    // Camera Image View Model
    public class CameraImageViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public CameraType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastActive { get; set; }
        public bool SupportsSnapshot { get; set; }
        public bool SupportsPanTilt { get; set; }
        public bool SupportsZoom { get; set; }
        public string TypeDisplayName => Type.ToString();
        public string StatusClass => IsActive ? "text-success" : "text-danger";
        public string StatusText => IsActive ? "Online" : "Offline";
    }

    // Live Stream View Model
    public class LiveStreamViewModel
    {
        public int CameraId { get; set; }
        public string CameraName { get; set; }
        public string StreamUrl { get; set; }
        public StreamQuality Quality { get; set; }
        public bool SupportsPanTilt { get; set; }
        public bool SupportsZoom { get; set; }
    }

    // Camera Grid View Model
    public class CameraGridViewModel
    {
        public List<CameraGridItemViewModel> Cameras { get; set; } = new();
        public int GridSize => Cameras.Count switch
        {
            1 => 1,
            <= 4 => 2,
            <= 9 => 3,
            <= 16 => 4,
            _ => 4
        };
    }

    // Camera Grid Item View Model
    public class CameraGridItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string StreamUrl { get; set; }
    }

    // Camera Configuration View Model
    public class CameraConfigurationViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Camera Name")]
        public string Name { get; set; }
        
        [Display(Name = "Location")]
        public string Location { get; set; }
        
        [Required]
        [Display(Name = "IP Address")]
        public string IpAddress { get; set; }
        
        [Display(Name = "Port")]
        [Range(1, 65535)]
        public int Port { get; set; } = 80;
        
        [Display(Name = "Username")]
        public string Username { get; set; }
        
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Display(Name = "Camera Type")]
        public CameraType Type { get; set; }
        
        [Display(Name = "Brand")]
        public string Brand { get; set; }
        
        [Display(Name = "Model")]
        public string Model { get; set; }
        
        [Display(Name = "Main Stream URL")]
        public string MainStreamUrl { get; set; }
        
        [Display(Name = "Sub Stream URL")]
        public string SubStreamUrl { get; set; }
        
        [Display(Name = "Snapshot URL")]
        public string SnapshotUrl { get; set; }
        
        // Capabilities
        [Display(Name = "Supports Pan/Tilt")]
        public bool SupportsPanTilt { get; set; }
        
        [Display(Name = "Supports Zoom")]
        public bool SupportsZoom { get; set; }
        
        [Display(Name = "Supports Night Vision")]
        public bool SupportsNightVision { get; set; }
        
        [Display(Name = "Supports Motion Detection")]
        public bool SupportsMotionDetection { get; set; }
        
        [Display(Name = "Supports Audio")]
        public bool SupportsAudio { get; set; }
        
        [Display(Name = "Supports Recording")]
        public bool SupportsRecording { get; set; }
    }
}

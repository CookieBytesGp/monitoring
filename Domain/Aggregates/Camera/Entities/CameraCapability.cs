
using Domain.SeedWork;
using Domain.Aggregates.Camera.ValueObjects;

namespace Domain.Aggregates.Camera.Entities
{
    /// <summary>
    /// قابلیت دوربین - Entity
    /// کاربرد: مدیریت قابلیت‌های مختلف دوربین (Pan/Tilt, Zoom, etc.)
    /// علت نیاز: هر دوربین می‌تواند قابلیت‌های مختلفی داشته باشد که باید مدیریت شوند
    /// </summary>
    public class CameraCapability : Entity
    {
        private CameraCapability(CapabilityType type, bool isEnabled, string configuration = null)
        {
            Id = Guid.NewGuid();
            Type = type;
            IsEnabled = isEnabled;
            Configuration = configuration;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public CapabilityType Type { get; private set; }
        public bool IsEnabled { get; private set; }
        public string Configuration { get; private set; } // JSON configuration specific to capability
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        public static CameraCapability Create(CapabilityType type, bool isEnabled = true, string configuration = null)
        {
            return new CameraCapability(type, isEnabled, configuration);
        }

        public void Enable()
        {
            IsEnabled = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Disable()
        {
            IsEnabled = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateConfiguration(string newConfiguration)
        {
            Configuration = newConfiguration;
            UpdatedAt = DateTime.UtcNow;
        }

        public string GetDisplayName()
        {
            return Type switch
            {
                CapabilityType.PanTilt => "Pan/Tilt Control",
                CapabilityType.Zoom => "Zoom Control",
                CapabilityType.NightVision => "Night Vision",
                CapabilityType.MotionDetection => "Motion Detection",
                CapabilityType.Audio => "Audio Recording",
                CapabilityType.Recording => "Video Recording",
                CapabilityType.TwoWayAudio => "Two-Way Audio",
                CapabilityType.PresetPositions => "Preset Positions",
                CapabilityType.DigitalZoom => "Digital Zoom",
                CapabilityType.AutoFocus => "Auto Focus",
                _ => Type.ToString()
            };
        }

        public bool RequiresConfiguration()
        {
            return Type switch
            {
                CapabilityType.MotionDetection => true,
                CapabilityType.Recording => true,
                CapabilityType.PresetPositions => true,
                _ => false
            };
        }
    }
}

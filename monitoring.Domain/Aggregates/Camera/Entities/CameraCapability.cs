
using Domain.Aggregates.Camera.ValueObjects;
using Monitoring.Domain.SeedWork;

namespace Domain.Aggregates.Camera.Entities;

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
        if (Type == CapabilityType.PanTilt) return "Pan/Tilt Control";
        if (Type == CapabilityType.Zoom) return "Zoom Control";
        if (Type == CapabilityType.NightVision) return "Night Vision";
        if (Type == CapabilityType.MotionDetection) return "Motion Detection";
        if (Type == CapabilityType.Audio) return "Audio Recording";
        if (Type == CapabilityType.Recording) return "Video Recording";
        if (Type == CapabilityType.TwoWayAudio) return "Two-Way Audio";
        if (Type == CapabilityType.PresetPositions) return "Preset Positions";
        if (Type == CapabilityType.DigitalZoom) return "Digital Zoom";
        if (Type == CapabilityType.AutoFocus) return "Auto Focus";
        return Type.ToString();
    }

    public bool RequiresConfiguration()
    {
        return Type == CapabilityType.MotionDetection
            || Type == CapabilityType.Recording
            || Type == CapabilityType.PresetPositions;
    }
}

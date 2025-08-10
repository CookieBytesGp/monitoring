namespace Domain.Aggregates.Camera.ValueObjects
{
    /// <summary>
    /// نوع دوربین - Enum
    /// کاربرد: تعریف انواع مختلف دوربین‌های قابل پشتیبانی
    /// </summary>
    public enum CameraType
    {
        IP = 1,
        RTSP = 2,
        ONVIF = 3,
        USB = 4,
        Analog = 5,
        Webcam = 6
    }

    /// <summary>
    /// وضعیت دوربین - Enum
    /// کاربرد: مدیریت حالت‌های مختلف دوربین
    /// </summary>
    public enum CameraStatus
    {
        Inactive = 0,
        Active = 1,
        Error = 2,
        Maintenance = 3,
        Connecting = 4
    }

    /// <summary>
    /// کیفیت stream - Enum
    /// کاربرد: تعریف سطوح مختلف کیفیت برای stream
    /// </summary>
    public enum StreamQuality
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Ultra = 4
    }

    /// <summary>
    /// نوع قابلیت دوربین - Enum
    /// کاربرد: تعریف انواع قابلیت‌های دوربین
    /// </summary>
    public enum CapabilityType
    {
        PanTilt = 1,
        Zoom = 2,
        NightVision = 3,
        MotionDetection = 4,
        Audio = 5,
        Recording = 6,
        TwoWayAudio = 7,
        PresetPositions = 8,
        DigitalZoom = 9,
        AutoFocus = 10
    }
}

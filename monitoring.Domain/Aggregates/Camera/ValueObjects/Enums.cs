using Monitoring.Domain.SeedWork;

namespace Monitoring.Domain.Aggregates.Camera.ValueObjects
{
    /// <summary>
    /// نوع دوربین - Enumeration
    /// </summary>
    public class CameraType : Enumeration
    {
        public static readonly CameraType IP = new(1, "IP");
        public static readonly CameraType RTSP = new(2, "RTSP");
        public static readonly CameraType ONVIF = new(3, "ONVIF");
        public static readonly CameraType USB = new(4, "USB");
        public static readonly CameraType Analog = new(5, "Analog");
        public static readonly CameraType Webcam = new(6, "Webcam");

        private CameraType()
        {
            
        }
        private CameraType(int value, string name) : base(value, name) { }
    }

    /// <summary>
    /// وضعیت دوربین - Enumeration
    /// </summary>
    public class CameraStatus : Enumeration
    {
    public static readonly CameraStatus Inactive = new(0, "Inactive");
    public static readonly CameraStatus Active = new(1, "Active");
    public static readonly CameraStatus Error = new(2, "Error");
    public static readonly CameraStatus Maintenance = new(3, "Maintenance");
    public static readonly CameraStatus Connecting = new(4, "Connecting");

        private CameraStatus()
        {
            
        }
        private CameraStatus(int value, string name) : base(value, name) { }
    }

    /// <summary>
    /// کیفیت stream - Enumeration
    /// </summary>
    public class StreamQuality : Enumeration
    {
    public static readonly StreamQuality Low = new(1, "Low");
    public static readonly StreamQuality Medium = new(2, "Medium");
    public static readonly StreamQuality High = new(3, "High");
    public static readonly StreamQuality Ultra = new(4, "Ultra");

        private StreamQuality()
        {
            
        }
        private StreamQuality(int value, string name) : base(value, name) { }
    }

    /// <summary>
    /// نوع قابلیت دوربین - Enumeration
    /// </summary>
    public class CapabilityType : Enumeration
    {
    public static readonly CapabilityType PanTilt = new(1, "PanTilt");
    public static readonly CapabilityType Zoom = new(2, "Zoom");
    public static readonly CapabilityType NightVision = new(3, "NightVision");
    public static readonly CapabilityType MotionDetection = new(4, "MotionDetection");
    public static readonly CapabilityType Audio = new(5, "Audio");
    public static readonly CapabilityType Recording = new(6, "Recording");
    public static readonly CapabilityType TwoWayAudio = new(7, "TwoWayAudio");
    public static readonly CapabilityType PresetPositions = new(8, "PresetPositions");
    public static readonly CapabilityType DigitalZoom = new(9, "DigitalZoom");
    public static readonly CapabilityType AutoFocus = new(10, "AutoFocus");

        private CapabilityType()
        {
            
        }
        private CapabilityType(int value, string name) : base(value, name) { }
    }

    /// <summary>
    /// نوع شبکه - Enumeration
    /// </summary>
    public class NetworkType : Enumeration
    {
        public static readonly NetworkType HTTP = new(1, "HTTP");
        public static readonly NetworkType RTSP = new(2, "RTSP");
        public static readonly NetworkType ONVIF = new(3, "ONVIF");

        private NetworkType()
        {
            
        }
        private NetworkType(int value, string name) : base(value, name) { }
    }

    /// <summary>
    /// کیفیت ضبط - Enumeration
    /// </summary>
    public class RecordingQuality : Enumeration
    {
        public static readonly RecordingQuality Low = new(1, "Low");
        public static readonly RecordingQuality Medium = new(2, "Medium");
        public static readonly RecordingQuality High = new(3, "High");
        public static readonly RecordingQuality UltraHigh = new(4, "UltraHigh");

        private RecordingQuality()
        {
            
        }
        private RecordingQuality(int value, string name) : base(value, name) { }
    }
}

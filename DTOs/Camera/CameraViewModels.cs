using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Camera
{
    public class CameraViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string LocationZone { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public Domain.Aggregates.Camera.CameraType Type { get; set; }
        public Domain.Aggregates.Camera.CameraStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActiveAt { get; set; }
        public bool IsOnline { get; set; }
    }

    public class CameraSummaryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public Domain.Aggregates.Camera.CameraStatus Status { get; set; }
        public Domain.Aggregates.Camera.CameraType Type { get; set; }
        public bool IsOnline { get; set; }
    }

    public class CreateCameraCommand
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string LocationZone { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Domain.Aggregates.Camera.CameraType Type { get; set; }
    }

    public class UpdateCameraCommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string LocationZone { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class CameraConfigurationViewModel
    {
        public Guid Id { get; set; }
        public string Resolution { get; set; }
        public int FrameRate { get; set; }
        public string VideoCodec { get; set; }
        public int Bitrate { get; set; }
        public bool AudioEnabled { get; set; }
        public string AudioCodec { get; set; }
        public bool MotionDetectionEnabled { get; set; }
        public int MotionSensitivity { get; set; }
        public bool RecordingEnabled { get; set; }
    }
}

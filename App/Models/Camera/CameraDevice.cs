using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using App.Models;

namespace App.Models.Camera
{
    public class CameraDevice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Location { get; set; }

        [Required]
        [StringLength(45)]
        public string IpAddress { get; set; }

        public int Port { get; set; } = 80;

        [Required]
        [StringLength(500)]
        public string StreamUrl { get; set; }

        // Additional stream URLs for different qualities
        [StringLength(500)]
        public string MainStreamUrl { get; set; }

        [StringLength(500)]
        public string SubStreamUrl { get; set; }

        [StringLength(500)]
        public string SnapshotUrl { get; set; }

        [StringLength(100)]
        public string Credentials { get; set; }

        [StringLength(100)]
        public string Username { get; set; }

        [StringLength(100)]
        public string Password { get; set; }

        [StringLength(20)]
        public string Resolution { get; set; }

        // Camera Type and Brand Information
        public CameraType Type { get; set; } = CameraType.IP;

        [StringLength(50)]
        public string Brand { get; set; }

        [StringLength(100)]
        public string Model { get; set; }

        // Configuration stored as JSON for flexibility
        public string ConfigurationJson { get; set; }

        // Camera Capabilities
        public bool SupportsPanTilt { get; set; }
        public bool SupportsZoom { get; set; }
        public bool SupportsNightVision { get; set; }
        public bool SupportsMotionDetection { get; set; }
        public bool SupportsAudio { get; set; }
        public bool SupportsRecording { get; set; }

        public bool IsActive { get; set; }

        public DateTime LastActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation property for motion events
        public virtual ICollection<MotionEvent> MotionEvents { get; set; }

        public CameraDevice()
        {
            IsActive = true;
            LastActive = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            MotionEvents = new List<MotionEvent>();
        }
    }

    // Camera Types Enum
    public enum CameraType
    {
        IP = 1,
        USB = 2,
        Analog = 3,
        RTSP = 4,
        HTTP = 5,
        ONVIF = 6,
        Webcam = 7
    }

    // Stream Quality Enum
    public enum StreamQuality
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Ultra = 4
    }
}

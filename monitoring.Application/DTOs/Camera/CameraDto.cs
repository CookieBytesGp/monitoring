using System;
using System.Collections.Generic;

namespace Monitoring.Application.DTOs.Camera
{
    public class CameraDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public CameraConfigurationDto Configuration { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastActiveAt { get; set; }
        public List<CameraStreamDto> Streams { get; set; }
        public List<CameraCapabilityDto> Capabilities { get; set; }
    }
}

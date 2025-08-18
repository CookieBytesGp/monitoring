using System;
using System.Collections.Generic;

namespace Monitoring.Application.DTOs.Camera
{
    public class CameraCapabilityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsSupported { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}

using System;

namespace Monitoring.Application.DTOs.Camera
{
    public class CameraStreamDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public string Quality { get; set; }
        public bool IsActive { get; set; }
    }
}

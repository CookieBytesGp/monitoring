namespace Monitoring.Application.DTOs.Camera
{
    public class CreateCameraDto
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 554;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Type { get; set; } = "HTTP";
        public string StreamUrl { get; set; } = string.Empty;
        public string SnapshotUrl { get; set; } = string.Empty;
    }
}

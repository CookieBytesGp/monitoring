using App.Models.Camera;
using System.Threading.Tasks;

namespace App.Services.Camera
{
    // Strategy Interface for different camera types
    public interface ICameraStreamStrategy
    {
        Task<string> GetStreamUrlAsync(CameraDevice camera, StreamQuality quality = StreamQuality.High);
        Task<byte[]> CaptureSnapshotAsync(CameraDevice camera);
        Task<bool> TestConnectionAsync(CameraDevice camera);
        bool SupportsCamera(CameraDevice camera);
    }

    // IP Camera Strategy
    public class IPCameraStrategy : ICameraStreamStrategy
    {
        public async Task<string> GetStreamUrlAsync(CameraDevice camera, StreamQuality quality = StreamQuality.High)
        {
            var baseUrl = $"http://{camera.IpAddress}:{camera.Port}";
            
            return quality switch
            {
                StreamQuality.High => camera.MainStreamUrl ?? $"{baseUrl}/video1",
                StreamQuality.Medium => camera.SubStreamUrl ?? $"{baseUrl}/video2", 
                StreamQuality.Low => $"{baseUrl}/video3",
                _ => camera.StreamUrl
            };
        }

        public async Task<byte[]> CaptureSnapshotAsync(CameraDevice camera)
        {
            using var httpClient = new HttpClient();
            
            if (!string.IsNullOrEmpty(camera.Username))
            {
                var authValue = Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes($"{camera.Username}:{camera.Password}"));
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
            }

            var snapshotUrl = camera.SnapshotUrl ?? 
                             $"http://{camera.IpAddress}:{camera.Port}/snapshot.jpg";
            
            return await httpClient.GetByteArrayAsync(snapshotUrl);
        }

        public async Task<bool> TestConnectionAsync(CameraDevice camera)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                
                var testUrl = $"http://{camera.IpAddress}:{camera.Port}";
                var response = await httpClient.GetAsync(testUrl);
                
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public bool SupportsCamera(CameraDevice camera)
        {
            return camera.Type == CameraType.IP || camera.Type == CameraType.HTTP;
        }
    }

    // RTSP Camera Strategy
    public class RTSPCameraStrategy : ICameraStreamStrategy
    {
        public async Task<string> GetStreamUrlAsync(CameraDevice camera, StreamQuality quality = StreamQuality.High)
        {
            var credentials = !string.IsNullOrEmpty(camera.Username) 
                ? $"{camera.Username}:{camera.Password}@" 
                : "";
            
            var baseUrl = $"rtsp://{credentials}{camera.IpAddress}:{camera.Port}";
            
            return quality switch
            {
                StreamQuality.High => camera.MainStreamUrl ?? $"{baseUrl}/stream1",
                StreamQuality.Medium => camera.SubStreamUrl ?? $"{baseUrl}/stream2",
                StreamQuality.Low => $"{baseUrl}/stream3",
                _ => camera.StreamUrl
            };
        }

        public async Task<byte[]> CaptureSnapshotAsync(CameraDevice camera)
        {
            // For RTSP cameras, we might need to use FFmpeg or similar tool
            // This is a placeholder implementation
            throw new NotImplementedException("RTSP snapshot capture requires FFmpeg integration");
        }

        public async Task<bool> TestConnectionAsync(CameraDevice camera)
        {
            try
            {
                // Simple TCP connection test for RTSP
                using var tcpClient = new System.Net.Sockets.TcpClient();
                await tcpClient.ConnectAsync(camera.IpAddress, camera.Port);
                return tcpClient.Connected;
            }
            catch
            {
                return false;
            }
        }

        public bool SupportsCamera(CameraDevice camera)
        {
            return camera.Type == CameraType.RTSP;
        }
    }

    // ONVIF Camera Strategy
    public class ONVIFCameraStrategy : ICameraStreamStrategy
    {
        public async Task<string> GetStreamUrlAsync(CameraDevice camera, StreamQuality quality = StreamQuality.High)
        {
            // ONVIF cameras require profile discovery
            // This is a simplified implementation
            var baseUrl = $"http://{camera.IpAddress}:{camera.Port}/onvif";
            
            return quality switch
            {
                StreamQuality.High => camera.MainStreamUrl ?? $"{baseUrl}/media_service/streaming/channels/1",
                StreamQuality.Medium => camera.SubStreamUrl ?? $"{baseUrl}/media_service/streaming/channels/2",
                StreamQuality.Low => $"{baseUrl}/media_service/streaming/channels/3",
                _ => camera.StreamUrl
            };
        }

        public async Task<byte[]> CaptureSnapshotAsync(CameraDevice camera)
        {
            using var httpClient = new HttpClient();
            
            // ONVIF authentication setup
            if (!string.IsNullOrEmpty(camera.Username))
            {
                var authValue = Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes($"{camera.Username}:{camera.Password}"));
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
            }

            var snapshotUrl = camera.SnapshotUrl ?? 
                             $"http://{camera.IpAddress}:{camera.Port}/onvif/media_service/snapshot";
            
            return await httpClient.GetByteArrayAsync(snapshotUrl);
        }

        public async Task<bool> TestConnectionAsync(CameraDevice camera)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                
                var onvifUrl = $"http://{camera.IpAddress}:{camera.Port}/onvif/device_service";
                var response = await httpClient.GetAsync(onvifUrl);
                
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public bool SupportsCamera(CameraDevice camera)
        {
            return camera.Type == CameraType.ONVIF;
        }
    }
}

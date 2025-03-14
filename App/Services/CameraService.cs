 using App.Services.Interfaces;
using App.Models.Camera;
using App.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Data;

namespace App.Services
{
    public class CameraService : ICameraService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<CameraHub> _cameraHub;
        private readonly ILogger<CameraService> _logger;
        private readonly HttpClient _httpClient;

        public CameraService(
            ApplicationDbContext context,
            IHubContext<CameraHub> cameraHub,
            ILogger<CameraService> logger,
            HttpClient httpClient)
        {
            _context = context;
            _cameraHub = cameraHub;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<CameraDevice>> GetAllCamerasAsync()
        {
            return await _context.Cameras
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<CameraDevice> GetCameraByIdAsync(int id)
        {
            return await _context.Cameras.FindAsync(id);
        }

        public async Task<CameraDevice> CreateCameraAsync(CameraDevice camera)
        {
            _context.Cameras.Add(camera);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Camera created: {camera.Name}");
            return camera;
        }

        public async Task<CameraDevice> UpdateCameraAsync(CameraDevice camera)
        {
            _context.Entry(camera).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Camera updated: {camera.Name}");
            return camera;
        }

        public async Task<bool> DeleteCameraAsync(int id)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
                return false;

            _context.Cameras.Remove(camera);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Camera deleted: {camera.Name}");
            return true;
        }

        public async Task<bool> UpdateCameraStatusAsync(int id, bool isActive)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
                return false;

            camera.IsActive = isActive;
            camera.LastActive = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _cameraHub.Clients.All.SendAsync("UpdateCameraStatus", id, isActive);
            _logger.LogInformation($"Camera {id} status updated to {isActive}");
            return true;
        }

        public async Task<Stream> GetCameraStreamAsync(int id)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
                throw new ArgumentException("Camera not found");

            try
            {
                // This is a placeholder for actual RTSP stream handling
                // In a real implementation, you would use a proper RTSP library
                var stream = await _httpClient.GetStreamAsync(camera.StreamUrl);
                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting camera stream for {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> TestCameraConnectionAsync(int id)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
                return false;

            try
            {
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = await ping.SendPingAsync(camera.IpAddress);
                var isActive = reply.Status == System.Net.NetworkInformation.IPStatus.Success;

                await UpdateCameraStatusAsync(id, isActive);
                return isActive;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error testing camera connection {id}: {ex.Message}");
                await UpdateCameraStatusAsync(id, false);
                return false;
            }
        }

        public async Task<string> GetCameraSnapshotAsync(int id)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
                throw new ArgumentException("Camera not found");

            try
            {
                // This is a placeholder for actual snapshot capture
                // In a real implementation, you would use ONVIF or camera-specific API
                return $"data:image/jpeg;base64,{Convert.ToBase64String(await _httpClient.GetByteArrayAsync(camera.StreamUrl))}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting camera snapshot for {id}: {ex.Message}");
                throw;
            }
        }
    }
}
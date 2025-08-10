using App.Models.Camera;
using App.Services.Camera;
using System.Collections.Generic;
using System.Linq;

namespace App.Services.Camera
{
    // Factory for creating appropriate camera strategies
    public class CameraStrategyFactory : ICameraStrategyFactory
    {
        private readonly List<ICameraStreamStrategy> _strategies;

        public CameraStrategyFactory()
        {
            _strategies = new List<ICameraStreamStrategy>
            {
                new IPCameraStrategy(),
                new RTSPCameraStrategy(),
                new ONVIFCameraStrategy()
            };
        }

        public ICameraStreamStrategy GetStrategy(CameraDevice camera)
        {
            var strategy = _strategies.FirstOrDefault(s => s.SupportsCamera(camera));
            
            if (strategy == null)
            {
                throw new NotSupportedException($"Camera type {camera.Type} is not supported");
            }
            
            return strategy;
        }

        public List<ICameraStreamStrategy> GetAllStrategies()
        {
            return _strategies;
        }
    }

    public interface ICameraStrategyFactory
    {
        ICameraStreamStrategy GetStrategy(CameraDevice camera);
        List<ICameraStreamStrategy> GetAllStrategies();
    }

    // Enhanced Camera Service with Strategy Pattern
    public class EnhancedCameraService : ICameraService
    {
        private readonly ICameraStrategyFactory _strategyFactory;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EnhancedCameraService> _logger;

        public EnhancedCameraService(
            ICameraStrategyFactory strategyFactory,
            ApplicationDbContext context,
            ILogger<EnhancedCameraService> logger)
        {
            _strategyFactory = strategyFactory;
            _context = context;
            _logger = logger;
        }

        public async Task<string> GetCameraStreamUrlAsync(int cameraId, StreamQuality quality = StreamQuality.High)
        {
            var camera = await _context.Cameras.FindAsync(cameraId);
            if (camera == null)
                throw new ArgumentException("Camera not found");

            var strategy = _strategyFactory.GetStrategy(camera);
            return await strategy.GetStreamUrlAsync(camera, quality);
        }

        public async Task<byte[]> CaptureSnapshotAsync(int cameraId)
        {
            var camera = await _context.Cameras.FindAsync(cameraId);
            if (camera == null)
                throw new ArgumentException("Camera not found");

            var strategy = _strategyFactory.GetStrategy(camera);
            return await strategy.CaptureSnapshotAsync(camera);
        }

        public async Task<bool> TestCameraConnectionAsync(int cameraId)
        {
            var camera = await _context.Cameras.FindAsync(cameraId);
            if (camera == null)
                return false;

            var strategy = _strategyFactory.GetStrategy(camera);
            var isConnected = await strategy.TestConnectionAsync(camera);
            
            // Update camera status
            camera.IsActive = isConnected;
            camera.LastActive = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return isConnected;
        }

        // Existing methods from original ICameraService
        public async Task<IEnumerable<CameraDevice>> GetAllCamerasAsync()
        {
            return await _context.Cameras.OrderBy(c => c.Name).ToListAsync();
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
            camera.UpdatedAt = DateTime.UtcNow;
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
    }
}

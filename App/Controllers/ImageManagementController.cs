using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Models.Camera;
using App.Services.Camera;
using App.Models.ViewModels;
using System.Text.Json;

namespace App.Controllers
{
    [Authorize]
    public class ImageManagementController : Controller
    {
        private readonly EnhancedCameraService _cameraService;
        private readonly ILogger<ImageManagementController> _logger;

        public ImageManagementController(
            EnhancedCameraService cameraService,
            ILogger<ImageManagementController> logger)
        {
            _cameraService = cameraService;
            _logger = logger;
        }

        // Main Image Management Panel
        public async Task<IActionResult> Index()
        {
            var cameras = await _cameraService.GetAllCamerasAsync();
            
            var viewModel = new ImageManagementViewModel
            {
                Cameras = cameras.Select(c => new CameraImageViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Location = c.Location,
                    Brand = c.Brand,
                    Model = c.Model,
                    Type = c.Type,
                    IsActive = c.IsActive,
                    LastActive = c.LastActive,
                    SupportsSnapshot = !string.IsNullOrEmpty(c.SnapshotUrl),
                    SupportsPanTilt = c.SupportsPanTilt,
                    SupportsZoom = c.SupportsZoom
                }).ToList()
            };

            return View(viewModel);
        }

        // Live Stream View
        [HttpGet]
        public async Task<IActionResult> LiveStream(int cameraId, StreamQuality quality = StreamQuality.High)
        {
            try
            {
                var camera = await _cameraService.GetCameraByIdAsync(cameraId);
                if (camera == null)
                    return NotFound();

                var streamUrl = await _cameraService.GetCameraStreamUrlAsync(cameraId, quality);
                
                var viewModel = new LiveStreamViewModel
                {
                    CameraId = cameraId,
                    CameraName = camera.Name,
                    StreamUrl = streamUrl,
                    Quality = quality,
                    SupportsPanTilt = camera.SupportsPanTilt,
                    SupportsZoom = camera.SupportsZoom
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting live stream for camera {CameraId}", cameraId);
                return StatusCode(500, "Error retrieving live stream");
            }
        }

        // Capture Snapshot
        [HttpPost]
        public async Task<IActionResult> CaptureSnapshot(int cameraId)
        {
            try
            {
                var imageData = await _cameraService.CaptureSnapshotAsync(cameraId);
                return File(imageData, "image/jpeg", $"snapshot_{cameraId}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing snapshot for camera {CameraId}", cameraId);
                return Json(new { success = false, message = "Error capturing snapshot" });
            }
        }

        // Get Stream URL API
        [HttpGet]
        public async Task<IActionResult> GetStreamUrl(int cameraId, StreamQuality quality = StreamQuality.High)
        {
            try
            {
                var streamUrl = await _cameraService.GetCameraStreamUrlAsync(cameraId, quality);
                return Json(new { success = true, streamUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream URL for camera {CameraId}", cameraId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Test Camera Connection
        [HttpPost]
        public async Task<IActionResult> TestConnection(int cameraId)
        {
            try
            {
                var isConnected = await _cameraService.TestCameraConnectionAsync(cameraId);
                return Json(new { success = true, connected = isConnected });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection for camera {CameraId}", cameraId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Multi-Camera Grid View
        [HttpGet]
        public async Task<IActionResult> GridView()
        {
            var cameras = await _cameraService.GetAllCamerasAsync();
            var activeCameras = cameras.Where(c => c.IsActive).ToList();

            var viewModel = new CameraGridViewModel
            {
                Cameras = activeCameras.Select(c => new CameraGridItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Location = c.Location,
                    StreamUrl = c.StreamUrl // We might need to get actual stream URL based on quality
                }).ToList()
            };

            return View(viewModel);
        }

        // Camera Configuration
        [HttpGet]
        public async Task<IActionResult> Configure(int cameraId)
        {
            var camera = await _cameraService.GetCameraByIdAsync(cameraId);
            if (camera == null)
                return NotFound();

            var viewModel = new CameraConfigurationViewModel
            {
                Id = camera.Id,
                Name = camera.Name,
                Location = camera.Location,
                IpAddress = camera.IpAddress,
                Port = camera.Port,
                Username = camera.Username,
                Password = camera.Password,
                Type = camera.Type,
                Brand = camera.Brand,
                Model = camera.Model,
                MainStreamUrl = camera.MainStreamUrl,
                SubStreamUrl = camera.SubStreamUrl,
                SnapshotUrl = camera.SnapshotUrl,
                SupportsPanTilt = camera.SupportsPanTilt,
                SupportsZoom = camera.SupportsZoom,
                SupportsNightVision = camera.SupportsNightVision,
                SupportsMotionDetection = camera.SupportsMotionDetection,
                SupportsAudio = camera.SupportsAudio,
                SupportsRecording = camera.SupportsRecording
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(CameraConfigurationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var camera = await _cameraService.GetCameraByIdAsync(model.Id);
                if (camera == null)
                    return NotFound();

                // Update camera properties
                camera.Name = model.Name;
                camera.Location = model.Location;
                camera.IpAddress = model.IpAddress;
                camera.Port = model.Port;
                camera.Username = model.Username;
                camera.Password = model.Password;
                camera.Type = model.Type;
                camera.Brand = model.Brand;
                camera.Model = model.Model;
                camera.MainStreamUrl = model.MainStreamUrl;
                camera.SubStreamUrl = model.SubStreamUrl;
                camera.SnapshotUrl = model.SnapshotUrl;
                camera.SupportsPanTilt = model.SupportsPanTilt;
                camera.SupportsZoom = model.SupportsZoom;
                camera.SupportsNightVision = model.SupportsNightVision;
                camera.SupportsMotionDetection = model.SupportsMotionDetection;
                camera.SupportsAudio = model.SupportsAudio;
                camera.SupportsRecording = model.SupportsRecording;

                await _cameraService.UpdateCameraAsync(camera);
                
                TempData["SuccessMessage"] = "Camera configuration updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating camera configuration for {CameraId}", model.Id);
                ModelState.AddModelError("", "Error updating camera configuration");
                return View(model);
            }
        }
    }
}

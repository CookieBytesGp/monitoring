using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Services.Interfaces;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class MotionDetectionController : Controller
    {
        private readonly IMotionDetectionService _motionDetectionService;
        private readonly ICameraService _cameraService;
        private readonly ILoggingService _loggingService;

        public MotionDetectionController(
            IMotionDetectionService motionDetectionService,
            ICameraService cameraService,
            ILoggingService loggingService)
        {
            _motionDetectionService = motionDetectionService;
            _cameraService = cameraService;
            _loggingService = loggingService;
        }

        public async Task<IActionResult> Index()
        {
            var cameras = await _cameraService.GetAllCamerasAsync();
            return View(cameras);
        }

        public async Task<IActionResult> Configure(string id)
        {
            if (!int.TryParse(id, out int cameraId))
            {
                return BadRequest("Invalid camera ID");
            }

            var camera = await _cameraService.GetCameraByIdAsync(cameraId);
            if (camera == null)
            {
                return NotFound();
            }

            var settings = await _motionDetectionService.GetSettingsAsync(id);
            ViewBag.Camera = camera;
            return View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSettings(string cameraId, MotionDetectionSettings settings)
        {
            try
            {
                await _motionDetectionService.UpdateSensitivityAsync(cameraId, settings.Sensitivity);
                await _motionDetectionService.UpdateRegionOfInterestAsync(cameraId, settings.RegionOfInterest);

                if (settings.IsActive)
                {
                    await _motionDetectionService.StartDetectionAsync(cameraId);
                }
                else
                {
                    await _motionDetectionService.StopDetectionAsync(cameraId);
                }

                await _loggingService.LogSystemEventAsync(
                    "MotionDetection",
                    $"Updated motion detection settings for camera {cameraId}");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "MotionDetection", $"Error updating settings for camera {cameraId}");
                return Json(new { success = false, message = "Failed to update settings" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDetection(string cameraId, bool enable)
        {
            try
            {
                if (enable)
                {
                    await _motionDetectionService.StartDetectionAsync(cameraId);
                }
                else
                {
                    await _motionDetectionService.StopDetectionAsync(cameraId);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "MotionDetection", $"Error toggling detection for camera {cameraId}");
                return Json(new { success = false, message = "Failed to toggle detection" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus(string cameraId)
        {
            var isActive = await _motionDetectionService.IsDetectionActiveAsync(cameraId);
            var settings = await _motionDetectionService.GetSettingsAsync(cameraId);
            
            return Json(new
            {
                isActive,
                settings
            });
        }
    }
}
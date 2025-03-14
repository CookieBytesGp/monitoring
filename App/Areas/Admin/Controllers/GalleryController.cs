using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Services.Interfaces;
using System.Linq;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class GalleryController : Controller
    {
        private readonly IMotionAnalyticsService _analyticsService;
        private readonly ICameraService _cameraService;

        public GalleryController(
            IMotionAnalyticsService analyticsService,
            ICameraService cameraService)
        {
            _analyticsService = analyticsService;
            _cameraService = cameraService;
        }

        public async Task<IActionResult> Index(string cameraId = null, DateTime? start = null, DateTime? end = null)
        {
            ViewBag.Cameras = await _cameraService.GetAllCamerasAsync();
            ViewBag.SelectedCameraId = cameraId;
            ViewBag.StartDate = start ?? DateTime.UtcNow.AddDays(-7);
            ViewBag.EndDate = end ?? DateTime.UtcNow;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetImages(
            string cameraId = null,
            DateTime? start = null,
            DateTime? end = null,
            int page = 1,
            int pageSize = 20,
            string sortBy = "timestamp",
            bool ascending = false)
        {
            var events = await _analyticsService.GetMotionEventsAsync(
                start ?? DateTime.UtcNow.AddDays(-7),
                end ?? DateTime.UtcNow,
                cameraId);

            var totalEvents = await _analyticsService.GetMotionEventCountAsync(
                cameraId,
                start ?? DateTime.UtcNow.AddDays(-7),
                end ?? DateTime.UtcNow);

            return Json(new
            {
                events = events.Select(e => new
                {
                    e.Id,
                    e.CameraId,
                    e.CameraName,
                    e.Timestamp,
                    e.MotionPercentage,
                    e.ImagePath,
                    e.Location,
                    e.Acknowledged
                }),
                totalEvents,
                currentPage = page,
                totalPages = (int)Math.Ceiling(totalEvents / (double)pageSize)
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var motionEvent = await _analyticsService.GetEventByIdAsync(id);
            if (motionEvent == null)
            {
                return NotFound();
            }

            // Get events around the same time (5 minutes before and after)
            var timeRange = await _analyticsService.GetMotionEventsAsync(
                motionEvent.Timestamp.AddMinutes(-5),
                motionEvent.Timestamp.AddMinutes(5),
                motionEvent.CameraId.ToString());

            ViewBag.RelatedEvents = timeRange.Where(e => e.Id != id).ToList();
            return View(motionEvent);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            try
            {
                await _analyticsService.DeleteEventAsync(id);
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkDelete(int[] ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    await _analyticsService.DeleteEventAsync(id);
                }
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var motionEvent = await _analyticsService.GetEventByIdAsync(id);
            if (motionEvent == null || string.IsNullOrEmpty(motionEvent.ImagePath))
            {
                return NotFound();
            }

            var fileName = $"motion_event_{motionEvent.CameraName}_{motionEvent.Timestamp:yyyyMMdd_HHmmss}.jpg";
            return PhysicalFile(motionEvent.ImagePath, "image/jpeg", fileName);
        }
    }
}
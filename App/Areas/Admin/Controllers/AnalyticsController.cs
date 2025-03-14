using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Services.Interfaces;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class AnalyticsController : Controller
    {
        private readonly IMotionAnalyticsService _analyticsService;
        private readonly ICameraService _cameraService;

        public AnalyticsController(
            IMotionAnalyticsService analyticsService,
            ICameraService cameraService)
        {
            _analyticsService = analyticsService;
            _cameraService = cameraService;
        }

        public async Task<IActionResult> Index()
        {
            var cameras = await _cameraService.GetAllCamerasAsync();
            return View(cameras);
        }

        public async Task<IActionResult> CameraAnalytics(string id)
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

            ViewBag.Camera = camera;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAnalytics(string cameraId, DateTime start, DateTime end)
        {
            var analytics = await _analyticsService.GetAnalyticsAsync(cameraId, start, end);
            return Json(analytics);
        }

        [HttpGet]
        public async Task<IActionResult> GetEventsByHour(string cameraId, DateTime date)
        {
            var events = await _analyticsService.GetEventsByHourAsync(cameraId, date);
            return Json(events);
        }

        [HttpGet]
        public async Task<IActionResult> GetEventsByDay(string cameraId, DateTime start, DateTime end)
        {
            var events = await _analyticsService.GetEventsByDayAsync(cameraId, start, end);
            return Json(events);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentEvents(int count = 100)
        {
            var events = await _analyticsService.GetRecentEventsAsync(count);
            return Json(events);
        }

        [HttpGet]
        public async Task<IActionResult> GetEventCountByCamera(DateTime start, DateTime end)
        {
            var counts = await _analyticsService.GetEventCountByCamera(start, end);
            return Json(counts);
        }

        [HttpPost]
        public async Task<IActionResult> AcknowledgeEvent(int eventId)
        {
            try
            {
                await _analyticsService.AcknowledgeEventAsync(eventId, User.Identity.Name);
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }
    }
}
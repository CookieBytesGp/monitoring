 using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Models.Monitor;
using App.Models.Camera;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Monitors()
        {
            return View();
        }

        public IActionResult Cameras()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        [HttpGet]
        public IActionResult MonitorStatus()
        {
            // Will implement real-time monitor status
            return Json(new { status = "success" });
        }

        [HttpGet]
        public IActionResult CameraFeeds()
        {
            // Will implement real-time camera feeds
            return Json(new { status = "success" });
        }
    }
}
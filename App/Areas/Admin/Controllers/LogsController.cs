 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Services.Interfaces;
using System.Text.Json;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class LogsController : Controller
    {
        private readonly ILoggingService _loggingService;

        public LogsController(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentLogs(string type = null, int count = 100)
        {
            var logs = await _loggingService.GetRecentLogsAsync(type, count);
            return Json(logs);
        }

        [HttpGet]
        public async Task<IActionResult> GetLogsByDateRange(DateTime start, DateTime end, string type = null)
        {
            var logs = await _loggingService.GetLogsByDateRangeAsync(start, end, type);
            return Json(logs);
        }

        [HttpGet]
        public IActionResult GetLogCategories()
        {
            var categories = new[]
            {
                "All",
                "System",
                "Security",
                "Device",
                "User",
                "Error"
            };
            return Json(categories);
        }

        [HttpGet]
        public IActionResult GetSeverityLevels()
        {
            var severities = new[]
            {
                "All",
                "Information",
                "Warning",
                "Error",
                "Critical"
            };
            return Json(severities);
        }
    }
}
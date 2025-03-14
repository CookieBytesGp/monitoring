using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Models.ViewModels;
using App.Services.Interfaces;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using App.Models;
using System.Diagnostics;

namespace App.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMotionAnalyticsService _analyticsService;
        private readonly ICameraService _cameraService;
        private readonly ILoggingService _loggingService;

        public HomeController(
            ApplicationDbContext context,
            IMotionAnalyticsService analyticsService,
            ICameraService cameraService,
            ILoggingService loggingService)
        {
            _context = context;
            _analyticsService = analyticsService;
            _cameraService = cameraService;
            _loggingService = loggingService;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            var dashboardViewModel = new DashboardViewModel
            {
                // Camera Statistics
                TotalCameras = await _context.Cameras.CountAsync(),
                ActiveCameras = await _context.Cameras.CountAsync(c => c.IsActive),
                InactiveCameras = await _context.Cameras.CountAsync(c => !c.IsActive),

                // Motion Events
                TodayEvents = await _context.MotionEvents
                    .CountAsync(e => e.Timestamp >= today),
                YesterdayEvents = await _context.MotionEvents
                    .CountAsync(e => e.Timestamp >= yesterday && e.Timestamp < today),
                UnacknowledgedEvents = await _context.MotionEvents
                    .CountAsync(e => !e.Acknowledged),

                // Recent Events
                RecentEvents = await _context.MotionEvents
                    .Include(e => e.Camera)
                    .OrderByDescending(e => e.Timestamp)
                    .Take(5)
                    .Select(e => new RecentEventViewModel
                    {
                        Id = e.Id,
                        CameraName = e.CameraName,
                        Timestamp = e.Timestamp,
                        MotionPercentage = e.MotionPercentage,
                        Location = e.Location,
                        IsAcknowledged = e.Acknowledged,
                        HasImage = !string.IsNullOrEmpty(e.ImagePath)
                    })
                    .ToListAsync(),

                // Active Cameras List
                ActiveCamerasList = await _context.Cameras
                    .Where(c => c.IsActive)
                    .Select(c => new ActiveCameraViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Location = c.Location,
                        LastActive = c.LastActive,
                        StreamUrl = c.StreamUrl
                    })
                    .ToListAsync(),

                // System Health
                SystemHealth = new SystemHealthViewModel
                {
                    LastError = await _context.SystemLogs
                        .Where(l => l.Severity == "Error")
                        .OrderByDescending(l => l.Timestamp)
                        .Select(l => new SystemErrorViewModel
                        {
                            Timestamp = l.Timestamp,
                            Message = l.Message,
                            Source = l.Source
                        })
                        .FirstOrDefaultAsync(),

                    RecentAlerts = await _context.SystemLogs
                        .Where(l => l.Severity == "Warning" || l.Severity == "Error")
                        .OrderByDescending(l => l.Timestamp)
                        .Take(5)
                        .Select(l => new SystemAlertViewModel
                        {
                            Timestamp = l.Timestamp,
                            Message = l.Message,
                            Severity = l.Severity
                        })
                        .ToListAsync()
                }
            };

            // Get hourly event distribution for today
            var hourlyEvents = await _context.MotionEvents
                .Where(e => e.Timestamp >= today)
                .GroupBy(e => e.Timestamp.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Hour, x => x.Count);

            dashboardViewModel.HourlyEventDistribution = Enumerable.Range(0, 24)
                .Select(hour => hourlyEvents.ContainsKey(hour) ? hourlyEvents[hour] : 0)
                .ToList();

            return View(dashboardViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetLiveStats()
        {
            var now = DateTime.UtcNow;
            var lastHour = now.AddHours(-1);

            var stats = new
            {
                ActiveCameras = await _context.Cameras.CountAsync(c => c.IsActive),
                RecentEvents = await _context.MotionEvents.CountAsync(e => e.Timestamp >= lastHour),
                UnacknowledgedEvents = await _context.MotionEvents.CountAsync(e => !e.Acknowledged),
                SystemErrors = await _context.SystemLogs.CountAsync(l => l.Severity == "Error" && l.Timestamp >= lastHour)
            };

            return Json(stats);
        }

        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

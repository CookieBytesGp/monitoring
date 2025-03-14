 using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using App.Models.Settings;
using System.Threading.Tasks;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public SettingsController(
            ILogger<SettingsController> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var settings = new SystemSettings
            {
                MonitorRefreshInterval = _configuration.GetValue<int>("Settings:MonitorRefreshInterval", 30),
                CameraRefreshInterval = _configuration.GetValue<int>("Settings:CameraRefreshInterval", 5),
                DefaultTheme = _configuration.GetValue<string>("Settings:DefaultTheme", "light"),
                EnableMotionDetection = _configuration.GetValue<bool>("Settings:EnableMotionDetection", true),
                MotionDetectionSensitivity = _configuration.GetValue<int>("Settings:MotionDetectionSensitivity", 5),
                EnableEmailNotifications = _configuration.GetValue<bool>("Settings:EnableEmailNotifications", false),
                NotificationEmail = _configuration.GetValue<string>("Settings:NotificationEmail", ""),
                AutoReconnectDevices = _configuration.GetValue<bool>("Settings:AutoReconnectDevices", true),
                LogRetentionDays = _configuration.GetValue<int>("Settings:LogRetentionDays", 30),
                MaxConcurrentStreams = _configuration.GetValue<int>("Settings:MaxConcurrentStreams", 10)
            };

            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SystemSettings settings)
        {
            if (!ModelState.IsValid)
                return View(settings);

            try
            {
                var configPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
                var json = await System.IO.File.ReadAllTextAsync(configPath);
                var jsonSettings = System.Text.Json.JsonDocument.Parse(json).RootElement;
                var settingsElement = jsonSettings.GetProperty("Settings");

                var updatedSettings = new Dictionary<string, object>
                {
                    { "MonitorRefreshInterval", settings.MonitorRefreshInterval },
                    { "CameraRefreshInterval", settings.CameraRefreshInterval },
                    { "DefaultTheme", settings.DefaultTheme },
                    { "EnableMotionDetection", settings.EnableMotionDetection },
                    { "MotionDetectionSensitivity", settings.MotionDetectionSensitivity },
                    { "EnableEmailNotifications", settings.EnableEmailNotifications },
                    { "NotificationEmail", settings.NotificationEmail },
                    { "AutoReconnectDevices", settings.AutoReconnectDevices },
                    { "LogRetentionDays", settings.LogRetentionDays },
                    { "MaxConcurrentStreams", settings.MaxConcurrentStreams }
                };

                var updatedJson = System.Text.Json.JsonSerializer.Serialize(
                    new { Settings = updatedSettings },
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                );

                await System.IO.File.WriteAllTextAsync(configPath, updatedJson);

                _logger.LogInformation("System settings updated successfully");
                TempData["SuccessMessage"] = "Settings updated successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating system settings");
                ModelState.AddModelError("", "Error saving settings. Please try again.");
                return View(settings);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTheme(string theme)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Update user's theme preference in the database
                // This would be implemented in your user service
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
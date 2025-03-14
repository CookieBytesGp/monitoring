using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using App.Models.Auth;
using App.Models.Theme;
using System.Threading.Tasks;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class UserSettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSettingsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.AvailableThemes = ThemeSettings.AvailableThemes;
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTheme(string theme)
        {
            if (!ThemeSettings.IsValidTheme(theme))
            {
                return BadRequest("Invalid theme selection");
            }

            var user = await _userManager.GetUserAsync(User);
            user.Theme = theme;
            await _userManager.UpdateAsync(user);

            return Json(new { success = true });
        }
    }
} 
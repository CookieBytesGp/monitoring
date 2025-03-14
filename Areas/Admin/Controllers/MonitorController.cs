using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using App.Models;
using System.Threading.Tasks;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MonitorController : Controller
    {
        private readonly ILogger<MonitorController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public MonitorController(
            ILogger<MonitorController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CheckRole()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Content("User not logged in");
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            return Content($"User: {user.Email}, Roles: {string.Join(", ", roles)}");
        }

        // ... rest of your existing actions ...
    }
} 
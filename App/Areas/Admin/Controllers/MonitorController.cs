 using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Models.Monitor;
using App.Models.Auth;
using Microsoft.AspNetCore.Identity;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MonitorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<MonitorController> _logger;

        public MonitorController(ILogger<MonitorController> logger , UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new MonitorDevice());
        }

        [HttpPost]
        public IActionResult Create(MonitorDevice monitor)
        {
            if (ModelState.IsValid)
            {
                // Will implement monitor creation
                return RedirectToAction(nameof(Index));
            }
            return View(monitor);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Will implement monitor editing
            return View();
        }

        [HttpPost]
        public IActionResult Edit(int id, MonitorDevice monitor)
        {
            if (ModelState.IsValid)
            {
                // Will implement monitor updating
                return RedirectToAction(nameof(Index));
            }
            return View(monitor);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            // Will implement monitor deletion
            return RedirectToAction(nameof(Index));
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
    }
}
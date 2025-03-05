using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using DTOs.Pagebuilder;

namespace App.Controllers
{
    public class PageController : Controller
    {
        private readonly PageService _pageService;

        public PageController(PageService pageService)
        {
            _pageService = pageService;
        }

        // GET: Page
        public async Task<IActionResult> Index()
        {
            try
            {
                var pages = await _pageService.GetPagesAsync();
                return View(pages);
            }
            catch (HttpRequestException ex)
            {
                var errorViewModel = new ErrorViewModel
                {
                    ErrorMessage = $"Request error: {ex.Message}"
                };
                return View("Error", errorViewModel);
            }
        }

        // GET: Page/Edit/5
        public IActionResult Edit(Guid id)
        {
            // Redirect to the PageEditorController's Edit action
            return RedirectToAction("Edit", "PageEditor", new { id });
        }


        // POST: Page/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _pageService.DeletePageAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}

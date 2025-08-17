using Microsoft.AspNetCore.Mvc;
using Monitoring.Ui.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Monitoring.Ui.Controllers
{
    public class PageController : Controller
    {
        private readonly IPageApiService _pageApiService;
        private readonly ILogger<PageController> _logger;

        public PageController(IPageApiService pageApiService, ILogger<PageController> logger)
        {
            _pageApiService = pageApiService;
            _logger = logger;
        }

        // صفحه لیست Pages
        public async Task<IActionResult> Index()
        {
            try
            {
                var pages = await _pageApiService.GetAllPagesAsync();
                return View(pages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pages list");
                TempData["Error"] = "خطای غیرمنتظره در دریافت لیست صفحات";
                return View(new List<PageDTO>());
            }
        }

        // صفحه جزئیات Page
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var page = await _pageApiService.GetPageByIdAsync(id);
                if (page != null)
                {
                    return View(page);
                }
                
                TempData["Error"] = "صفحه مورد نظر یافت نشد";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting page details for {PageId}", id);
                TempData["Error"] = "خطا در دریافت جزئیات صفحه";
                return RedirectToAction(nameof(Index));
            }
        }

        // صفحه ایجاد Page جدید
        public IActionResult Create()
        {
            var model = new CreatePageViewModel
            {
                DisplayWidth = 1920,
                DisplayHeight = 1080,
                Orientation = "Landscape"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePageViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _pageApiService.CreatePageAsync(
                    model.Title,
                    model.DisplayWidth,
                    model.DisplayHeight,
                    model.Orientation);

                if (result != null)
                {
                    TempData["Success"] = "صفحه با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Edit), new { id = result.Id });
                }

                ModelState.AddModelError("", "خطا در ایجاد صفحه");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating page");
                ModelState.AddModelError("", "خطا در ایجاد صفحه");
                return View(model);
            }
        }

        // صفحه ویرایش جزئیات Page
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var page = await _pageApiService.GetPageByIdAsync(id);
                if (page != null)
                {
                    var model = new EditPageViewModel
                    {
                        Id = page.Id,
                        Title = page.Title,
                        DisplayWidth = page.DisplayConfig.Width,
                        DisplayHeight = page.DisplayConfig.Height,
                        Orientation = page.DisplayConfig.Orientation,
                        ThumbnailUrl = page.DisplayConfig.ThumbnailUrl,
                        Status = page.Status,
                        ElementsCount = page.ElementsCount,
                        HasBackgroundAsset = page.HasBackgroundAsset
                    };
                    return View(model);
                }
                
                TempData["Error"] = "صفحه مورد نظر یافت نشد";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting page for edit {PageId}", id);
                TempData["Error"] = "خطا در دریافت اطلاعات صفحه";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPageViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var success = await _pageApiService.UpdatePageAsync(
                    model.Id, 
                    model.Title, 
                    model.DisplayWidth, 
                    model.DisplayHeight, 
                    model.Orientation, 
                    model.ThumbnailUrl);

                if (success)
                {
                    TempData["Success"] = "جزئیات صفحه با موفقیت به‌روزرسانی شد";
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }

                ModelState.AddModelError("", "خطا در به‌روزرسانی صفحه");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {PageId}", model.Id);
                ModelState.AddModelError("", "خطا در به‌روزرسانی صفحه");
                return View(model);
            }
        }

        // حذف Page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _pageApiService.DeletePageAsync(id);
                if (success)
                {
                    TempData["Success"] = "صفحه با موفقیت حذف شد";
                }
                else
                {
                    TempData["Error"] = "خطا در حذف صفحه";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting page {PageId}", id);
                TempData["Error"] = "خطا در حذف صفحه";
            }

            return RedirectToAction(nameof(Index));
        }

        // تغییر وضعیت Page
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(Guid id, string status)
        {
            try
            {
                var success = await _pageApiService.TogglePageStatusAsync(id, status);
                if (success)
                {
                    return Json(new { success = true, message = "وضعیت با موفقیت تغییر کرد" });
                }
                
                return Json(new { success = false, message = "خطا در تغییر وضعیت" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling page status {PageId}", id);
                return Json(new { success = false, message = "خطا در تغییر وضعیت" });
            }
        }
    }

    // ViewModels
    public class CreatePageViewModel
    {
        [Required(ErrorMessage = "عنوان اجباری است")]
        [StringLength(200, ErrorMessage = "عنوان نباید بیش از 200 کاراکتر باشد")]
        public string Title { get; set; }

        [Required]
        [Range(1, 7680, ErrorMessage = "عرض باید بین 1 تا 7680 پیکسل باشد")]
        public int DisplayWidth { get; set; }

        [Required]
        [Range(1, 4320, ErrorMessage = "ارتفاع باید بین 1 تا 4320 پیکسل باشد")]
        public int DisplayHeight { get; set; }

        [Required]
        public string Orientation { get; set; }
    }

    public class EditPageViewModel : CreatePageViewModel
    {
        public Guid Id { get; set; }
        
        [Url(ErrorMessage = "آدرس تصویر بندانگشتی معتبر نیست")]
        public string ThumbnailUrl { get; set; }
        
        public string Status { get; set; }
        public int ElementsCount { get; set; }
        public bool HasBackgroundAsset { get; set; }
    }
}

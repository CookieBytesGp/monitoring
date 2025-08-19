using Microsoft.AspNetCore.Mvc;
using Monitoring.Ui.Interfaces;
using Monitoring.Ui.Models.Page;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
                return View(new List<PageViewModel>());
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

                var result = await _pageApiService.CreatePageAsync(new CreatePageRequest
                {
                    Title = model.Title,
                    DisplayWidth = model.DisplayWidth,
                    DisplayHeight = model.DisplayHeight,
                    Orientation = model.Orientation
                });

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
                        ThumbnailUrl = page.DisplayConfig.ThumbnailUrl, // Can be null
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

        // صفحه ویرایشگر بصری المنت‌ها
        public async Task<IActionResult> Editor(Guid id)
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
                _logger.LogError(ex, "Error getting page for editor {PageId}", id);
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

                // Force status to be Draft for edits (status changes should be done through settings)
                model.Status = "Draft";
                
                // Remove ThumbnailUrl from model to avoid issues (will be handled systematically later)
                model.ThumbnailUrl = null;

                // Update basic page info
                var updateRequest = new UpdatePageRequest
                {
                    Id = model.Id,
                    Title = model.Title
                };

                var basicUpdateSuccess = await _pageApiService.UpdatePageAsync(model.Id, updateRequest);
                if (!basicUpdateSuccess)
                {
                    TempData["ErrorMessage"] = "خطا در به‌روزرسانی اطلاعات پایه صفحه";
                    return View(model);
                }

                // Update display configuration (without thumbnail for now)
                var displayRequest = new UpdateDisplaySizeRequest
                {
                    Width = model.DisplayWidth,
                    Height = model.DisplayHeight,
                    Orientation = model.Orientation
                    // ThumbnailUrl will be handled systematically later
                };

                var displayUpdateSuccess = await _pageApiService.UpdateDisplaySizeAsync(model.Id, displayRequest);
                if (!displayUpdateSuccess)
                {
                    TempData["ErrorMessage"] = "خطا در به‌روزرسانی تنظیمات نمایش";
                    return View(model);
                }

                // Update thumbnail if provided
                if (!string.IsNullOrEmpty(model.ThumbnailUrl))
                {
                    var thumbnailRequest = new UpdateThumbnailRequest
                    {
                        ThumbnailUrl = model.ThumbnailUrl
                    };

                    var thumbnailUpdateSuccess = await _pageApiService.UpdateThumbnailAsync(model.Id, thumbnailRequest);
                    if (!thumbnailUpdateSuccess)
                    {
                        TempData["ErrorMessage"] = "خطا در به‌روزرسانی تصویر بندانگشتی";
                        return View(model);
                    }
                }

                TempData["SuccessMessage"] = "جزئیات صفحه با موفقیت به‌روزرسانی شد";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {PageId}", model.Id);
                TempData["ErrorMessage"] = "خطا در به‌روزرسانی صفحه. لطفاً دوباره تلاش کنید";
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
                var request = new UpdateStatusRequest { Status = status };
                var success = await _pageApiService.UpdateStatusAsync(id, request);
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
}

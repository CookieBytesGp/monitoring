using Microsoft.AspNetCore.Mvc;
using Monitoring.Ui.Services;
using Monitoring.Ui.Models.Camera;

namespace Monitoring.Ui.Controllers;

public class CameraController : Controller
{
    private readonly ICameraApiService _cameraApiService;
    private readonly ILogger<CameraController> _logger;

    public CameraController(ICameraApiService cameraApiService, ILogger<CameraController> logger)
    {
        _cameraApiService = cameraApiService;
        _logger = logger;
    }

    /// <summary>
    /// صفحه اصلی مدیریت دوربین‌ها
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var cameras = await _cameraApiService.GetAllCamerasAsync();
            return View(cameras);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading cameras page");
            ViewBag.ErrorMessage = "خطا در بارگذاری لیست دوربین‌ها";
            return View(new List<CameraViewModel>());
        }
    }

    /// <summary>
    /// صفحه نمایش جزئیات دوربین
    /// </summary>
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var camera = await _cameraApiService.GetCameraByIdAsync(id);
            
            if (camera == null)
            {
                TempData["ErrorMessage"] = "دوربین یافت نشد";
                return RedirectToAction(nameof(Index));
            }
            
            return View(camera);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading camera details {CameraId}", id);
            TempData["ErrorMessage"] = "خطا در بارگذاری جزئیات دوربین";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// صفحه ایجاد دوربین جدید
    /// </summary>
    public IActionResult Create()
    {
        return View(new CreateCameraViewModel());
    }

    /// <summary>
    /// ایجاد دوربین جدید
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCameraViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(model);

            var camera = await _cameraApiService.CreateCameraAsync(model);
            
            if (camera == null)
            {
                ViewBag.ErrorMessage = "خطا در ایجاد دوربین";
                return View(model);
            }
            
            TempData["SuccessMessage"] = "دوربین با موفقیت ایجاد شد";
            return RedirectToAction(nameof(Details), new { id = camera.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating camera");
            ViewBag.ErrorMessage = "خطا در ایجاد دوربین";
            return View(model);
        }
    }

    /// <summary>
    /// صفحه تست اتصال دوربین
    /// </summary>
    public async Task<IActionResult> TestConnection(Guid id)
    {
        try
        {
            var camera = await _cameraApiService.GetCameraByIdAsync(id);
            if (camera == null)
            {
                TempData["ErrorMessage"] = "دوربین یافت نشد";
                return RedirectToAction(nameof(Index));
            }

            var testResult = await _cameraApiService.TestConnectionAsync(id);
            
            ViewBag.CameraName = camera.Name;
            ViewBag.TestResult = testResult;
            ViewBag.TestMessage = testResult ? "اتصال موفق" : "اتصال ناموفق";
            
            return View(camera);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while testing camera connection {CameraId}", id);
            TempData["ErrorMessage"] = "خطا در تست اتصال دوربین";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// صفحه مشاهده ویدیو زنده دوربین
    /// </summary>
    public async Task<IActionResult> LiveView(Guid id)
    {
        try
        {
            var camera = await _cameraApiService.GetCameraByIdAsync(id);
            if (camera == null)
            {
                TempData["ErrorMessage"] = "دوربین یافت نشد";
                return RedirectToAction(nameof(Index));
            }

            // اتصال با بهترین استراتژی
            var connectionInfo = await _cameraApiService.ConnectAsync(id);
            ViewBag.ConnectionInfo = connectionInfo;

            // دریافت URL استریم
            var streamUrl = await _cameraApiService.GetStreamUrlAsync(id);
            ViewBag.StreamUrl = streamUrl;
            
            return View(camera);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading live view for camera {CameraId}", id);
            TempData["ErrorMessage"] = "خطا در بارگذاری تصویر زنده";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// دریافت عکس از دوربین
    /// </summary>
    public async Task<IActionResult> Snapshot(Guid id)
    {
        try
        {
            var imageBytes = await _cameraApiService.GetSnapshotAsync(id);
            
            if (imageBytes == null)
                return NotFound();
            
            return File(imageBytes, "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while capturing snapshot from camera {CameraId}", id);
            return NotFound();
        }
    }

    /// <summary>
    /// تست تمام استراتژی‌های دوربین
    /// </summary>
    public async Task<IActionResult> TestStrategies(Guid id)
    {
        try
        {
            var camera = await _cameraApiService.GetCameraByIdAsync(id);
            if (camera == null)
            {
                TempData["ErrorMessage"] = "دوربین یافت نشد";
                return RedirectToAction(nameof(Index));
            }

            var supportedStrategies = await _cameraApiService.GetSupportedStrategiesAsync(id);
            var testResults = await _cameraApiService.TestAllStrategiesAsync(id);
            
            var viewModel = new CameraTestResultViewModel
            {
                CameraName = camera.Name,
                SupportedStrategies = supportedStrategies,
                StrategyResults = testResults
            };
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while testing strategies for camera {CameraId}", id);
            TempData["ErrorMessage"] = "خطا در تست استراتژی‌ها";
            return RedirectToAction(nameof(Index));
        }
    }
}

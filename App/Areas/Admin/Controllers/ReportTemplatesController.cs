using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using App.Data;
using App.Models;
using App.Services.Interfaces;
using System.Collections.Generic;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class ReportTemplatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportGenerationService _reportService;
        private readonly IMotionAnalyticsService _analyticsService;
        private readonly IImageProcessingService _imageService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReportTemplatesController> _logger;

        public ReportTemplatesController(
            ApplicationDbContext context,
            IReportGenerationService reportService,
            IMotionAnalyticsService analyticsService,
            IImageProcessingService imageService,
            IEmailService emailService,
            ILogger<ReportTemplatesController> logger)
        {
            _context = context;
            _reportService = reportService;
            _analyticsService = analyticsService;
            _imageService = imageService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var templates = await _context.ReportTemplates
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return View(templates);
        }

        public IActionResult Create()
        {
            ViewBag.Cameras = _context.Cameras.OrderBy(c => c.Name).ToList();
            return View(new ReportTemplate { CreatedAt = DateTime.UtcNow });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReportTemplate template)
        {
            if (ModelState.IsValid)
            {
                template.CreatedAt = DateTime.UtcNow;
                _context.Add(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Cameras = _context.Cameras.OrderBy(c => c.Name).ToList();
            return View(template);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _context.ReportTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }
            ViewBag.Cameras = _context.Cameras.OrderBy(c => c.Name).ToList();
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReportTemplate template)
        {
            if (id != template.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTemplate = await _context.ReportTemplates.FindAsync(id);
                    if (existingTemplate == null)
                    {
                        return NotFound();
                    }

                    // Preserve creation date
                    template.CreatedAt = existingTemplate.CreatedAt;
                    _context.Entry(existingTemplate).State = EntityState.Detached;
                    _context.Update(template);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TemplateExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            ViewBag.Cameras = _context.Cameras.OrderBy(c => c.Name).ToList();
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var template = await _context.ReportTemplates.FindAsync(id);
                if (template == null)
                {
                    return NotFound();
                }

                _context.ReportTemplates.Remove(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template {TemplateId}", id);
                return StatusCode(500, new { success = false, message = "Failed to delete template" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> GenerateReport(int id, [FromBody] DateRangeModel dateRange)
        {
            try
            {
                var template = await _context.ReportTemplates.FindAsync(id);
                if (template == null)
                {
                    return NotFound();
                }

                var history = new ReportExportHistory
                {
                    ReportTemplateId = template.Id,
                    ExportedAt = DateTime.UtcNow,
                    Format = template.Format,
                    WasScheduled = false,
                    GeneratedBy = User.Identity.Name
                };

                try
                {
                    DateTime startDate, endDate;

                    if (dateRange != null && !string.IsNullOrEmpty(dateRange.StartDate) && !string.IsNullOrEmpty(dateRange.EndDate))
                    {
                        startDate = DateTime.Parse(dateRange.StartDate);
                        endDate = DateTime.Parse(dateRange.EndDate);
                    }
                    else
                    {
                        endDate = DateTime.UtcNow;
                        startDate = template.TimeRangeType switch
                        {
                            "Daily" => endDate.AddDays(-1),
                            "Weekly" => endDate.AddDays(-7),
                            "Monthly" => endDate.AddMonths(-1),
                            "Custom" => endDate.AddDays(-(template.CustomDays ?? 7)),
                            _ => endDate.AddDays(-7)
                        };
                    }

                    history.StartDate = startDate;
                    history.EndDate = endDate;

                    byte[] reportData;
                    switch (template.Format.ToUpper())
                    {
                        case "CSV":
                            reportData = await _reportService.GenerateCsvReportAsync(template, startDate, endDate);
                            break;
                        case "JSON":
                            reportData = await _reportService.GenerateJsonReportAsync(template, startDate, endDate);
                            break;
                        case "EXCEL":
                            reportData = await _reportService.GenerateExcelReportAsync(template, startDate, endDate);
                            break;
                        default:
                            return BadRequest("Unsupported format");
                    }

                    history.FileSizeBytes = reportData.Length;
                    var contentType = await _reportService.GetContentTypeForFormat(template.Format);
                    var fileExtension = await _reportService.GetFileExtensionForFormat(template.Format);
                    history.FileName = $"Report_{template.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{fileExtension}";

                    if (template.IsScheduled && !string.IsNullOrEmpty(template.EmailRecipients))
                    {
                        history.EmailRecipients = template.EmailRecipients;
                        var emailSent = await _reportService.SendReportEmailAsync(template, reportData, template.Format);
                        history.EmailSent = emailSent;

                        if (!emailSent)
                        {
                            _logger.LogWarning("Failed to send report email for template {TemplateId}", template.Id);
                        }
                    }

                    history.Status = "Success";
                    await _context.ReportExportHistory.AddAsync(history);
                    await _context.SaveChangesAsync();

                    if (history.EmailSent)
                    {
                        return Json(new { success = true, message = "Report generated and sent via email" });
                    }

                    return File(reportData, contentType, history.FileName);
                }
                catch (Exception ex)
                {
                    history.Status = "Failed";
                    history.ErrorMessage = ex.Message;
                    await _context.ReportExportHistory.AddAsync(history);
                    await _context.SaveChangesAsync();

                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report for template {TemplateId}", id);
                return StatusCode(500, new { success = false, message = "Failed to generate report" });
            }
        }

        private async Task<bool> TemplateExists(int id)
        {
            return await _context.ReportTemplates.AnyAsync(e => e.Id == id);
        }
        
        [HttpGet]
        public async Task<IActionResult> Preview(int id)
        {
            var template = await _context.ReportTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            // Get a sample of data (last 5 records) for preview
            var previewData = await GetPreviewData(template);
            return View(new ReportPreviewViewModel
            {
                Template = template,
                PreviewData = previewData,
                TimeRanges = GetTimeRangeOptions()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePreview(int id, [FromBody] PreviewRequestModel request)
        {
            try
            {
                var template = await _context.ReportTemplates.FindAsync(id);
                if (template == null)
                {
                    return NotFound();
                }

                var startDate = DateTime.Parse(request.StartDate);
                var endDate = DateTime.Parse(request.EndDate);

                // Generate preview data based on format
                byte[] previewData;
                switch (template.Format.ToUpper())
                {
                    case "CSV":
                        previewData = await _reportService.GenerateCsvReportAsync(template, startDate, endDate);
                        break;
                    case "JSON":
                        previewData = await _reportService.GenerateJsonReportAsync(template, startDate, endDate);
                        break;
                    case "EXCEL":
                        previewData = await _reportService.GenerateExcelReportAsync(template, startDate, endDate);
                        break;
                    default:
                        return BadRequest("Unsupported format");
                }

                var contentType = await _reportService.GetContentTypeForFormat(template.Format);
                var fileExtension = await _reportService.GetFileExtensionForFormat(template.Format);
                var fileName = $"Preview_{template.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{fileExtension}";

                return File(previewData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating preview for template {TemplateId}", id);
                return StatusCode(500, new { success = false, message = "Failed to generate preview" });
            }
        }

        private async Task<List<MotionEventPreviewModel>> GetPreviewData(ReportTemplate template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            try
            {
                IQueryable<MotionEvent> baseQuery = _context.MotionEvents;

                // Apply filters before Include
                if (!string.IsNullOrEmpty(template.CameraFilter))
                {
                    var cameraIds = template.CameraFilter
                        .Split(',')
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => int.TryParse(s.Trim(), out int id) ? id : -1)
                        .Where(id => id != -1)
                        .ToList();

                    if (cameraIds.Any())
                    {
                        baseQuery = baseQuery.Where(e => cameraIds.Contains(e.CameraId));
                    }
                }

                // Apply Include and ordering after filters
                var query = baseQuery
                    .Include(e => e.Camera)
                    .OrderByDescending(e => e.Timestamp)
                    .Take(5);

                var events = await query.ToListAsync();
                var previewData = new List<MotionEventPreviewModel>();

                foreach (var evt in events)
                {
                    if (evt?.Camera == null) continue; // Skip if event or camera is null

                    var previewModel = new MotionEventPreviewModel
                    {
                        EventId = evt.Id,
                        Timestamp = evt.Timestamp,
                        CameraName = evt.Camera.Name,
                        MotionPercentage = (double)evt.MotionPercentage,
                        IsAcknowledged = evt.Acknowledged
                    };

                    try
                    {
                        if (template.IncludeMotionStats)
                        {
                            var stats = await _analyticsService.GetMotionStatsAsync(evt.Id);
                            previewModel.MotionStats = stats;
                        }

                        if (template.IncludeImageAnalysis)
                        {
                            var analysis = await _imageService.AnalyzeImageQualityAsync(evt.ImagePath);
                            previewModel.ImageAnalysis = analysis;
                        }

                        previewData.Add(previewModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing preview data for event {EventId}", evt.Id);
                        // Continue processing other events even if one fails
                    }
                }

                return previewData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating preview data");
                throw;
            }
        }

        private List<TimeRangeOption> GetTimeRangeOptions()
        {
            return new List<TimeRangeOption>
            {
                new TimeRangeOption { Value = "Daily", Label = "Last 24 Hours" },
                new TimeRangeOption { Value = "Weekly", Label = "Last 7 Days" },
                new TimeRangeOption { Value = "Monthly", Label = "Last 30 Days" },
                new TimeRangeOption { Value = "Custom", Label = "Custom Range" }
            };
        }
        
        [HttpGet]
        public async Task<IActionResult> Duplicate(int id)
        {
            var template = await _context.ReportTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            var duplicatedTemplate = new ReportTemplate
            {
                Name = $"Copy of {template.Name}",
                Description = template.Description,
                Format = template.Format,
                CreatedAt = DateTime.UtcNow,
                IncludeImages = template.IncludeImages,
                IncludeMotionStats = template.IncludeMotionStats,
                IncludeImageAnalysis = template.IncludeImageAnalysis,
                IncludeProcessingHistory = template.IncludeProcessingHistory,
                TimeRangeType = template.TimeRangeType,
                CustomDays = template.CustomDays,
                CameraFilter = template.CameraFilter,
                MinMotionPercentage = template.MinMotionPercentage,
                MaxMotionPercentage = template.MaxMotionPercentage,
                OnlyAcknowledged = template.OnlyAcknowledged,
                GroupByCamera = template.GroupByCamera,
                GroupByDate = template.GroupByDate,
                IsScheduled = false // Reset scheduling for safety
            };

            ViewBag.Cameras = _context.Cameras.OrderBy(c => c.Name).ToList();
            ViewBag.SourceTemplateId = id;
            return View("Create", duplicatedTemplate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuplicateConfirm(int sourceId, ReportTemplate template)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    template.CreatedAt = DateTime.UtcNow;
                    template.IsScheduled = false; // Ensure scheduling is reset
                    
                    _context.Add(template);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Template {SourceId} duplicated successfully as {NewId}",
                        sourceId,
                        template.Id
                    );

                    TempData["Success"] = "Template duplicated successfully";
                    return RedirectToAction(nameof(Edit), new { id = template.Id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating template {SourceId}", sourceId);
                ModelState.AddModelError("", "An error occurred while duplicating the template");
            }

            ViewBag.Cameras = _context.Cameras.OrderBy(c => c.Name).ToList();
            ViewBag.SourceTemplateId = sourceId;
            return View("Create", template);
        }
        
        [HttpGet]
        public async Task<IActionResult> ExportHistory(int id)
        {
            var template = await _context.ReportTemplates
                .Include(t => t.ReportExportHistory)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            return View(new ExportHistoryViewModel
            {
                Template = template,
                History = await _context.ReportExportHistory
                    .Where(h => h.ReportTemplateId == id)
                    .OrderByDescending(h => h.ExportedAt)
                    .Take(100)
                    .ToListAsync()
            });
        }

        

    }

    public class DateRangeModel
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
    public class ReportPreviewViewModel
    {
        public ReportTemplate Template { get; set; }
        public List<MotionEventPreviewModel> PreviewData { get; set; }
        public List<TimeRangeOption> TimeRanges { get; set; }
    }

    public class MotionEventPreviewModel
    {
        public int EventId { get; set; }
        public DateTime Timestamp { get; set; }
        public string CameraName { get; set; }
        public double MotionPercentage { get; set; }
        public bool IsAcknowledged { get; set; }
        public dynamic MotionStats { get; set; }
        public dynamic ImageAnalysis { get; set; }
    }

    public class TimeRangeOption
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    public class PreviewRequestModel
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
    
    public class ExportHistoryViewModel
    {
        public ReportTemplate Template { get; set; }
        public List<ReportExportHistory> History { get; set; }
    }
}
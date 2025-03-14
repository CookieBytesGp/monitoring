 using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using App.Data;
using App.Models;
using System.Collections.Generic;
using App.Models.FilterModels;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class EmailTemplatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailTemplatesController> _logger;

        public EmailTemplatesController(
            ApplicationDbContext context,
            ILogger<EmailTemplatesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index([FromQuery] TemplateFilterModel filter)
        {
            var query = _context.EmailTemplates.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(t => 
                    t.Name.ToLower().Contains(searchTerm) ||
                    t.Subject.ToLower().Contains(searchTerm) ||
                    t.Description.ToLower().Contains(searchTerm));
            }

            // Apply IsDefault filter
            if (filter.IsDefault.HasValue)
            {
                query = query.Where(t => t.IsDefault == filter.IsDefault.Value);
            }

            // Apply date range filter
            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= filter.CreatedFrom.Value);
            }
            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= filter.CreatedTo.Value);
            }

            // Apply sorting
            query = ApplySorting(query, filter);

            // Get total count for pagination
            var totalItems = await query.CountAsync();

            // Apply pagination
            var templates = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var viewModel = new TemplateListViewModel
            {
                Templates = templates,
                Filter = filter,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize)
            };

            return View(viewModel);
        }

        private IQueryable<EmailTemplate> ApplySorting(IQueryable<EmailTemplate> query, TemplateFilterModel filter)
        {
            var sortBy = filter.SortBy?.ToLower() ?? TemplateFilterModel.SortOptions.CreatedAt;
            var sortDesc = filter.SortDescending;

            query = sortBy switch
            {
                TemplateFilterModel.SortOptions.Name => sortDesc 
                    ? query.OrderByDescending(t => t.Name)
                    : query.OrderBy(t => t.Name),
                
                TemplateFilterModel.SortOptions.LastModified => sortDesc
                    ? query.OrderByDescending(t => t.LastModifiedAt ?? t.CreatedAt)
                    : query.OrderBy(t => t.LastModifiedAt ?? t.CreatedAt),
                
                _ => sortDesc // Default to CreatedAt
                    ? query.OrderByDescending(t => t.CreatedAt)
                    : query.OrderBy(t => t.CreatedAt)
            };

            return query;
        }


        public IActionResult Create()
        {
            ViewBag.Placeholders = GetAvailablePlaceholders();
            return View(new EmailTemplate { CreatedAt = DateTime.UtcNow });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmailTemplate template)
        {
            if (ModelState.IsValid)
            {
                template.CreatedAt = DateTime.UtcNow;
                
                if (template.IsDefault)
                {
                    // Ensure only one default template exists
                    var existingDefault = await _context.EmailTemplates
                        .Where(t => t.IsDefault)
                        .FirstOrDefaultAsync();
                    
                    if (existingDefault != null)
                    {
                        existingDefault.IsDefault = false;
                        _context.Update(existingDefault);
                    }
                }

                _context.Add(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Placeholders = GetAvailablePlaceholders();
            return View(template);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            ViewBag.Placeholders = GetAvailablePlaceholders();
            return View(template);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmailTemplate template)
        {
            if (id != template.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    template.LastModifiedAt = DateTime.UtcNow;

                    if (template.IsDefault)
                    {
                        // Ensure only one default template exists
                        var existingDefault = await _context.EmailTemplates
                            .Where(t => t.IsDefault && t.Id != id)
                            .FirstOrDefaultAsync();
                        
                        if (existingDefault != null)
                        {
                            existingDefault.IsDefault = false;
                            _context.Update(existingDefault);
                        }
                    }

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

            ViewBag.Placeholders = GetAvailablePlaceholders();
            return View(template);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            try
            {
                // Check if it's the only default template
                if (template.IsDefault && await _context.EmailTemplates.CountAsync(t => t.IsDefault) <= 1)
                {
                    _logger.LogWarning("Attempted to delete the only default template {Id}", id);
                    return StatusCode(400, new { success = false, message = "Cannot delete the only default template. Please set another template as default first." });
                }

                _context.EmailTemplates.Remove(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template {Id}", id);
                return StatusCode(500, new { success = false, message = "Failed to delete template" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Preview(int id)
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            // Create sample data for preview
            var previewData = new
            {
                ReportName = "Sample Monthly Activity Report",
                DateRange = "01/01/2024 - 01/31/2024",
                GeneratedDate = DateTime.UtcNow.ToString("g"),
                RecordsCount = "150",
                FileFormat = "Excel",
                FileSize = "2.5 MB",
                GeneratedBy = User.Identity.Name
            };

            var subject = ReplacePlaceholders(template.Subject, previewData);
            var body = ReplacePlaceholders(template.Body, previewData);

            return View(new EmailPreviewViewModel
            {
                Template = template,
                PreviewSubject = subject,
                PreviewBody = body
            });
        }

        private string ReplacePlaceholders(string content, dynamic data)
        {
            return content
                .Replace(EmailTemplate.Placeholders.ReportName, data.ReportName)
                .Replace(EmailTemplate.Placeholders.DateRange, data.DateRange)
                .Replace(EmailTemplate.Placeholders.GeneratedDate, data.GeneratedDate)
                .Replace(EmailTemplate.Placeholders.RecordsCount, data.RecordsCount)
                .Replace(EmailTemplate.Placeholders.FileFormat, data.FileFormat)
                .Replace(EmailTemplate.Placeholders.FileSize, data.FileSize)
                .Replace(EmailTemplate.Placeholders.GeneratedBy, data.GeneratedBy);
        }

        private string[] GetAvailablePlaceholders()
        {
            return new[]
            {
                EmailTemplate.Placeholders.ReportName,
                EmailTemplate.Placeholders.DateRange,
                EmailTemplate.Placeholders.GeneratedDate,
                EmailTemplate.Placeholders.RecordsCount,
                EmailTemplate.Placeholders.FileFormat,
                EmailTemplate.Placeholders.FileSize,
                EmailTemplate.Placeholders.GeneratedBy
            };
        }

        private async Task<bool> TemplateExists(int id)
        {
            return await _context.EmailTemplates.AnyAsync(e => e.Id == id);
        }
    }

    public class EmailPreviewViewModel
    {
        public EmailTemplate Template { get; set; }
        public string PreviewSubject { get; set; }
        public string PreviewBody { get; set; }
    }
    
    public class TemplateListViewModel
    {
        public List<EmailTemplate> Templates { get; set; }
        public TemplateFilterModel Filter { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
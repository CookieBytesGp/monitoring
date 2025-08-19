using Microsoft.AspNetCore.Mvc;
using Monitoring.Application.DTOs.Page;
using DTOs.Pagebuilder;
using Domain.Aggregates.Page.ValueObjects;
using Monitoring.Domain.SeedWork;
using FluentResults;
using Monitoring.Application.Interfaces.Page;
using Monitoring.Application.Interfaces.Tool;

namespace Monitoring.ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PageController : ControllerBase
{
    private readonly IPageService _pageService;
    private readonly IToolService _toolService;
    private readonly ILogger<PageController> _logger;

    public PageController(IPageService pageService, IToolService toolService, ILogger<PageController> logger)
    {
        _pageService = pageService;
        _toolService = toolService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت لیست تمام صفحات
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllPages()
    {
        try
        {
            _logger.LogInformation("Getting all pages");
            
            var result = await _pageService.GetAllAsync();
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to get all pages: {Errors}", string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(new { 
                    success = false, 
                    message = "Failed to get pages", 
                    errors = result.Errors.Select(e => e.Message) 
                });
            }
            
            _logger.LogInformation("Retrieved {Count} pages successfully", result.Value.Count());
            return Ok(new { 
                success = true, 
                data = result.Value,
                count = result.Value.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all pages");
            return StatusCode(500, new { 
                success = false, 
                message = "Internal server error", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// تست ساده برای بررسی API
    /// </summary>
    [HttpGet("test")]
    public IActionResult TestEndpoint()
    {
        return Ok(new { 
            message = "API is working", 
            timestamp = DateTime.UtcNow,
            success = true 
        });
    }

    /// <summary>
    /// دریافت صفحه با ID مشخص
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPageById(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting page with ID: {PageId}", id);
            
            var result = await _pageService.GetPageAsync(id);
            
            if (result.IsFailed)
            {
                _logger.LogWarning("Page not found with ID: {PageId}", id);
                return NotFound(new { 
                    success = false, 
                    message = "Page not found", 
                    errors = result.Errors.Select(e => e.Message) 
                });
            }
            
            _logger.LogInformation("Page retrieved successfully: {PageId}", id);
            return Ok(new { 
                success = true, 
                data = result.Value 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting page {PageId}", id);
            return StatusCode(500, new { 
                success = false, 
                message = "Internal server error", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// ایجاد صفحه جدید
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePage([FromBody] CreatePageRequestDTO request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for creating page");
                return BadRequest(new { 
                    success = false, 
                    message = "Invalid input data", 
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                });
            }

            _logger.LogInformation("Creating page with Title: {Title}, DisplaySize: {Width}x{Height}, Orientation: {Orientation}", 
                request.Title, request.DisplayWidth, request.DisplayHeight, request.Orientation);

            // Parse orientation
            if (!Enumeration.TryGetFromValueOrName<DisplayOrientation>(request.Orientation, out var orientation))
            {
                _logger.LogWarning("Invalid orientation value: {Orientation}", request.Orientation);
                var validOrientations = new[] { "Portrait", "Landscape", "Square" };
                return BadRequest(new { 
                    success = false, 
                    message = $"Invalid orientation. Valid values are: {string.Join(", ", validOrientations)}" 
                });
            }

            var result = await _pageService.CreatePageAsync(
                request.Title, 
                request.DisplayWidth, 
                request.DisplayHeight, 
                orientation, 
                request.Elements);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to create page: {Errors}", string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(new { 
                    success = false, 
                    message = "Failed to create page", 
                    errors = result.Errors.Select(e => e.Message) 
                });
            }
            
            _logger.LogInformation("Page created successfully with Id: {PageId}", result.Value.Id);
            
            // Add additional debugging info
            _logger.LogInformation("Page DTO data: Title='{Title}', Status='{Status}', DisplayConfig='{DisplayConfig}'", 
                result.Value.Title, 
                result.Value.Status, 
                result.Value.DisplayConfig?.Orientation);
            
            return CreatedAtAction(nameof(GetPageById), new { id = result.Value.Id }, new { 
                success = true, 
                message = "Page created successfully", 
                data = result.Value 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating page");
            return StatusCode(500, new { 
                success = false, 
                message = "Internal server error", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// به‌روزرسانی صفحه
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePage(Guid id, [FromBody] UpdatePageRequestDTO request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating page {PageId}", id);
                return BadRequest(new { 
                    success = false, 
                    message = "Invalid input data", 
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                });
            }

            _logger.LogInformation("Updating page {PageId} with Title: {Title}", id, request.Title);

            var result = await _pageService.UpdatePageAsync(id, request.Title, request.Elements);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to update page {PageId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(new { 
                    success = false, 
                    message = "Failed to update page", 
                    errors = result.Errors.Select(e => e.Message) 
                });
            }
            
            _logger.LogInformation("Page {PageId} updated successfully", id);
            return Ok(new { 
                success = true, 
                message = "Page updated successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating page {PageId}", id);
            return StatusCode(500, new { 
                success = false, 
                message = "Internal server error", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// حذف صفحه
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePage(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting page {PageId}", id);

            var result = await _pageService.DeletePageAsync(id);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to delete page {PageId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(new { 
                    success = false, 
                    message = "Failed to delete page", 
                    errors = result.Errors.Select(e => e.Message) 
                });
            }
            
            _logger.LogInformation("Page {PageId} deleted successfully", id);
            return Ok(new { 
                success = true, 
                message = "Page deleted successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting page {PageId}", id);
            return StatusCode(500, new { 
                success = false, 
                message = "Internal server error", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// تنظیم وضعیت صفحه
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> SetPageStatus(Guid id, [FromBody] SetPageStatusRequestDTO request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for setting page status {PageId}", id);
                return BadRequest(new { 
                    success = false, 
                    message = "Invalid input data", 
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                });
            }

            _logger.LogInformation("Setting page {PageId} status to {Status}", id, request.Status);

            // Parse status
            if (!Enumeration.TryGetFromValueOrName<PageStatus>(request.Status, out var status))
            {
                _logger.LogWarning("Invalid status value: {Status}", request.Status);
                var validStatuses = new[] { "Draft", "Published", "Archived", "Scheduled" };
                return BadRequest(new { 
                    success = false, 
                    message = $"Invalid status. Valid values are: {string.Join(", ", validStatuses)}" 
                });
            }

            var result = await _pageService.SetPageStatusAsync(id, status);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to set page status {PageId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(new { 
                    success = false, 
                    message = "Failed to set page status", 
                    errors = result.Errors.Select(e => e.Message) 
                });
            }
            
            _logger.LogInformation("Page {PageId} status set to {Status} successfully", id, request.Status);
            return Ok(new { 
                success = true, 
                message = "Page status updated successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting page status {PageId}", id);
            return StatusCode(500, new { 
                success = false, 
                message = "Internal server error", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// تنظیم اندازه نمایش صفحه
    /// </summary>
    [HttpPut("{id}/display-size")]
    public async Task<IActionResult> SetPageDisplaySize(Guid id, [FromBody] SetPageDisplaySizeRequestDTO request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for setting page display size {PageId}", id);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Setting page {PageId} display size to {Width}x{Height}", id, request.Width, request.Height);

            Result result;
            
            if (!string.IsNullOrEmpty(request.Orientation))
            {
                // Parse orientation
                if (!Enumeration.TryGetFromValueOrName<DisplayOrientation>(request.Orientation, out var orientation))
                {
                    _logger.LogWarning("Invalid orientation value: {Orientation}", request.Orientation);
                    var validOrientations = new[] { "Portrait", "Landscape", "Square" };
                    return BadRequest($"Invalid orientation. Valid values are: {string.Join(", ", validOrientations)}");
                }
                
                result = await _pageService.SetPageDisplaySizeAsync(id, request.Width, request.Height, orientation);
            }
            else
            {
                result = await _pageService.SetPageDisplaySizeAsync(id, request.Width, request.Height);
            }
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to set page display size {PageId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(result.Errors.Select(e => e.Message));
            }
            
            _logger.LogInformation("Page {PageId} display size set successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting page display size {PageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// تنظیم thumbnail صفحه
    /// </summary>
    [HttpPut("{id}/thumbnail")]
    public async Task<IActionResult> SetPageThumbnail(Guid id, [FromBody] SetPageThumbnailRequestDTO request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for setting page thumbnail {PageId}", id);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Setting page {PageId} thumbnail to {ThumbnailUrl}", id, request.ThumbnailUrl);

            var result = await _pageService.SetPageThumbnailAsync(id, request.ThumbnailUrl);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to set page thumbnail {PageId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(result.Errors.Select(e => e.Message));
            }
            
            _logger.LogInformation("Page {PageId} thumbnail set successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting page thumbnail {PageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// تنظیم background asset صفحه
    /// </summary>
    [HttpPut("{id}/background-asset")]
    public async Task<IActionResult> SetBackgroundAsset(Guid id, [FromBody] AssetDTO assetDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for setting page background asset {PageId}", id);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Setting page {PageId} background asset", id);

            // Pass DTO directly to service layer for domain mapping
            var result = await _pageService.SetBackgroundAssetAsync(id, assetDto);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to set page background asset {PageId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(result.Errors.Select(e => e.Message));
            }
            
            _logger.LogInformation("Page {PageId} background asset set successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting page background asset {PageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// حذف background asset صفحه
    /// </summary>
    [HttpDelete("{id}/background-asset")]
    public async Task<IActionResult> RemoveBackgroundAsset(Guid id)
    {
        try
        {
            _logger.LogInformation("Removing page {PageId} background asset", id);

            var result = await _pageService.RemoveBackgroundAssetAsync(id);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to remove page background asset {PageId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(result.Errors.Select(e => e.Message));
            }
            
            _logger.LogInformation("Page {PageId} background asset removed successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing page background asset {PageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// اضافه کردن element به صفحه
    /// </summary>
    [HttpPost("{id}/elements")]
    public async Task<IActionResult> AddElement(Guid id, [FromBody] BaseElementDTO elementDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for adding element to page {PageId}", id);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Adding element to page {PageId}", id);

            var result = await _pageService.AddElementAsync(id, elementDto);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to add element to page {PageId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(result.Errors.Select(e => e.Message));
            }
            
            _logger.LogInformation("Element added to page {PageId} successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding element to page {PageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// حذف element از صفحه
    /// </summary>
    [HttpDelete("{id}/elements/{elementId}")]
    public async Task<IActionResult> RemoveElement(Guid id, Guid elementId)
    {
        try
        {
            _logger.LogInformation("Removing element {ElementId} from page {PageId}", elementId, id);

            var result = await _pageService.RemoveElementAsync(id, elementId);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to remove element {ElementId} from page {PageId}: {Errors}", elementId, id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(result.Errors.Select(e => e.Message));
            }
            
            _logger.LogInformation("Element {ElementId} removed from page {PageId} successfully", elementId, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing element {ElementId} from page {PageId}", elementId, id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// به‌روزرسانی element صفحه
    /// </summary>
    [HttpPut("{id}/elements/{elementId}")]
    public async Task<IActionResult> UpdateElement(Guid id, Guid elementId, [FromBody] BaseElementDTO elementDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating element {ElementId} in page {PageId}", elementId, id);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Updating element {ElementId} in page {PageId}", elementId, id);

            var result = await _pageService.UpdateElementAsync(id, elementId, elementDto);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to update element {ElementId} in page {PageId}: {Errors}", elementId, id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(result.Errors.Select(e => e.Message));
            }
            
            _logger.LogInformation("Element {ElementId} in page {PageId} updated successfully", elementId, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating element {ElementId} in page {PageId}", elementId, id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// تغییر ترتیب elements صفحه
    /// </summary>
    [HttpPut("{id}/elements/reorder")]
    public async Task<IActionResult> ReorderElements(Guid id, [FromBody] ReorderElementsRequestDTO request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for reordering elements in page {PageId}", id);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Reordering elements in page {PageId}", id);

            var orderChanges = request.OrderChanges.Select(x => (x.ElementId, x.NewOrder)).ToList();
            var result = await _pageService.ReorderElementsAsync(id, orderChanges);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to reorder elements in page {PageId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(result.Errors.Select(e => e.Message));
            }
            
            _logger.LogInformation("Elements reordered in page {PageId} successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reordering elements in page {PageId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// دریافت لیست ابزارها برای ادیتور
    /// </summary>
    [HttpGet("tools")]
    public async Task<IActionResult> GetTools()
    {
        try
        {
            var result = await _toolService.GetAllToolsAsync();
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to get tools: {Errors}", string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(new { 
                    success = false, 
                    message = "Failed to get tools", 
                    errors = result.Errors.Select(e => e.Message) 
                });
            }
            
            return Ok(new { 
                success = true, 
                data = result.Value,
                message = "Tools retrieved successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting tools");
            return StatusCode(500, new { 
                success = false, 
                message = "Internal server error", 
                error = ex.Message 
            });
        }
    }
}

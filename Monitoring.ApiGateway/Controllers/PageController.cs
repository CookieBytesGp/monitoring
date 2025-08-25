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
    //[HttpPut("{id}/background-asset")]
    //public async Task<IActionResult> SetBackgroundAsset(Guid id, [FromBody] AssetDTO assetDto)
    //{
    //    try
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            _logger.LogWarning("Invalid model state for setting page background asset {PageId}", id);
    //            return BadRequest(ModelState);
    //        }

    //        _logger.LogInformation("Setting page {PageId} background asset", id);

    //        // Pass DTO directly to service layer for domain mapping
    //        var result = await _pageService.SetBackgroundAssetAsync(id, assetDto);
            
    //        if (result.IsFailed)
    //        {
    //            _logger.LogError("Failed to set page background asset {PageId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Message)));
    //            return BadRequest(result.Errors.Select(e => e.Message));
    //        }
            
    //        _logger.LogInformation("Page {PageId} background asset set successfully", id);
    //        return NoContent();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error occurred while setting page background asset {PageId}", id);
    //        return StatusCode(500, "Internal server error");
    //    }
    //}

    ///// <summary>
    ///// حذف background asset صفحه
    ///// </summary>
    //[HttpDelete("{id}/background-asset")]
    //public async Task<IActionResult> RemoveBackgroundAsset(Guid id)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Removing page {PageId} background asset", id);

    //        var result = await _pageService.RemoveBackgroundAssetAsync(id);
            
    //        if (result.IsFailed)
    //        {
    //            _logger.LogError("Failed to remove page background asset {PageId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Message)));
    //            return BadRequest(result.Errors.Select(e => e.Message));
    //        }
            
    //        _logger.LogInformation("Page {PageId} background asset removed successfully", id);
    //        return NoContent();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error occurred while removing page background asset {PageId}", id);
    //        return StatusCode(500, "Internal server error");
    //    }
    //}

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
    /// ذخیره bulk elements از cache
    /// </summary>
    [HttpPost("{id}/elements/bulk")]
    public async Task<IActionResult> SaveElementsFromCache(Guid id, [FromBody] SaveElementsFromCacheRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for saving elements from cache for page {PageId}", id);
                return BadRequest(new { 
                    success = false, 
                    message = "Invalid input data", 
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                });
            }

            _logger.LogInformation("Saving {ElementCount} elements from cache for page {PageId}", 
                request.Elements?.Count ?? 0, id);

            // Transform cache elements to domain structure
            var domainElements = new List<BaseElementDTO>();
            
            if (request.Elements != null && request.Elements.Any())
            {
                foreach (var (cacheElement, index) in request.Elements.Select((e, i) => (e, i)))
                {
                    var domainElement = TransformCacheElementToDomain(cacheElement, index);
                    if (domainElement != null)
                    {
                        domainElements.Add(domainElement);
                    }
                }
            }

            // Update page with new elements
            var updateRequest = new UpdatePageRequestDTO
            {
                Title = request.PageTitle ?? "Page", // Use provided title or default
                Elements = domainElements
            };

            var result = await _pageService.UpdatePageAsync(id, updateRequest.Title, updateRequest.Elements);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to save elements from cache for page {PageId}: {Errors}", 
                    id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(new { 
                    success = false, 
                    message = "Failed to save elements from cache", 
                    errors = result.Errors.Select(e => e.Message) 
                });
            }
            
            _logger.LogInformation("Successfully saved {ElementCount} elements from cache for page {PageId}", 
                domainElements.Count, id);
                
            return Ok(new { 
                success = true, 
                message = "Elements saved successfully from cache",
                elementCount = domainElements.Count,
                pageId = id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving elements from cache for page {PageId}", id);
            return StatusCode(500, new { 
                success = false, 
                message = "Internal server error", 
                error = ex.Message 
            });
        }
    }

    private BaseElementDTO TransformCacheElementToDomain(CacheElementData cacheElement, int order)
    {
        try
        {
            // Map element type to ToolId
            var toolId = GetToolIdForElementType(cacheElement.Type);
            
            // Create TemplateBody from cache data
            var templateBody = new TemplateBodyDTO
            {
                HtmlTemplate = GenerateHtmlTemplateForType(cacheElement.Type),
                DefaultCssClasses = GenerateDefaultCssClassesForType(cacheElement.Type),
                CustomCss = GenerateCustomCssFromCache(cacheElement),
                CustomJs = "",
                IsFloating = false
            };
            
            // Create Asset from cache data
            var asset = CreateAssetFromCacheElement(cacheElement);
            
            return new BaseElementDTO
            {
                Id = Guid.TryParse(cacheElement.Id, out var parsedId) ? parsedId : Guid.NewGuid(),
                ToolId = toolId,
                Order = order,
                TemplateBody = templateBody,
                Asset = asset
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming cache element {ElementId} to domain", cacheElement.Id);
            return null;
        }
    }

    private Guid GetToolIdForElementType(string elementType)
    {
        // این نقشه باید از یک service یا configuration آمده باشد
        var toolMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
        {
            { "text", Guid.Parse("11111111-1111-1111-1111-111111111111") },
            { "image", Guid.Parse("22222222-2222-2222-2222-222222222222") },
            { "video", Guid.Parse("33333333-3333-3333-3333-333333333333") },
            { "camera", Guid.Parse("44444444-4444-4444-4444-444444444444") },
            { "clock", Guid.Parse("55555555-5555-5555-5555-555555555555") },
            { "weather", Guid.Parse("66666666-6666-6666-6666-666666666666") }
        };

        return toolMap.TryGetValue(elementType, out var toolId) 
            ? toolId 
            : Guid.Parse("11111111-1111-1111-1111-111111111111"); // Default to text
    }

    private string GenerateHtmlTemplateForType(string elementType)
    {
        return elementType.ToLower() switch
        {
            "text" => "<div class=\"text-element\">{{content}}</div>",
            "image" => "<img class=\"image-element\" src=\"{{src}}\" alt=\"{{alt}}\" />",
            "video" => "<video class=\"video-element\" src=\"{{src}}\" controls></video>",
            "camera" => "<div class=\"camera-element\"><i class=\"fas fa-camera\"></i><span>{{title}}</span></div>",
            "clock" => "<div class=\"clock-element\"><div class=\"clock-time\">{{time}}</div></div>",
            "weather" => "<div class=\"weather-element\"><div class=\"weather-info\">{{location}}</div></div>",
            _ => "<div class=\"unknown-element\">{{content}}</div>"
        };
    }

    private Dictionary<string, string> GenerateDefaultCssClassesForType(string elementType)
    {
        var classes = new Dictionary<string, string>
        {
            { "container", $"element-{elementType}" },
            { "position", "absolute" }
        };

        switch (elementType.ToLower())
        {
            case "text":
                classes.Add("text", "text-content");
                break;
            case "image":
                classes.Add("image", "image-content");
                break;
            case "video":
                classes.Add("video", "video-content");
                break;
        }

        return classes;
    }

    private string GenerateCustomCssFromCache(CacheElementData element)
    {
        var cssRules = new List<string>();

        // Position and size
        if (element.Position != null)
        {
            cssRules.Add($"left: {element.Position.X}px");
            cssRules.Add($"top: {element.Position.Y}px");
            cssRules.Add($"width: {element.Position.Width}px");
            cssRules.Add($"height: {element.Position.Height}px");
        }

        // Styles from cache
        if (element.Styles != null)
        {
            if (!string.IsNullOrEmpty(element.Styles.BackgroundColor))
                cssRules.Add($"background-color: {element.Styles.BackgroundColor}");

            if (!string.IsNullOrEmpty(element.Styles.Color))
                cssRules.Add($"color: {element.Styles.Color}");

            if (!string.IsNullOrEmpty(element.Styles.FontSize))
                cssRules.Add($"font-size: {element.Styles.FontSize}");

            if (!string.IsNullOrEmpty(element.Styles.FontFamily))
                cssRules.Add($"font-family: {element.Styles.FontFamily}");

            if (!string.IsNullOrEmpty(element.Styles.Border))
                cssRules.Add($"border: {element.Styles.Border}");

            if (!string.IsNullOrEmpty(element.Styles.BorderRadius))
                cssRules.Add($"border-radius: {element.Styles.BorderRadius}");
        }

        return string.Join("; ", cssRules) + (cssRules.Count > 0 ? ";" : "");
    }

    private AssetDTO CreateAssetFromCacheElement(CacheElementData element)
    {
        var assetDto = new AssetDTO
        {
            Metadata = new Dictionary<string, string>()
        };

        switch (element.Type.ToLower())
        {
            case "text":
                assetDto.Type = "text";
                assetDto.Content = element.Config?.Content ?? element.Content?.TextContent ?? "";
                assetDto.Url = null;
                break;

            case "image":
                assetDto.Type = "image";
                assetDto.Url = element.Config?.Src ?? "";
                assetDto.AltText = element.Config?.Alt ?? "";
                assetDto.Content = null;
                break;

            case "video":
                assetDto.Type = "video";
                assetDto.Url = element.Config?.Src ?? "";
                assetDto.Content = null;
                assetDto.Metadata["autoplay"] = element.Config?.Autoplay?.ToString() ?? "false";
                assetDto.Metadata["loop"] = element.Config?.Loop?.ToString() ?? "false";
                break;

            case "camera":
                assetDto.Type = "camera";
                assetDto.Content = element.Config?.Title ?? "دوربین";
                assetDto.Url = null;
                break;

            case "clock":
                assetDto.Type = "clock";
                assetDto.Content = "clock_widget";
                assetDto.Metadata["format"] = element.Config?.Format ?? "24";
                assetDto.Metadata["showSeconds"] = element.Config?.ShowSeconds?.ToString() ?? "true";
                break;

            case "weather":
                assetDto.Type = "weather";
                assetDto.Content = element.Config?.Location ?? "تهران";
                assetDto.Metadata["location"] = element.Config?.Location ?? "تهران";
                break;

            default:
                assetDto.Type = "unknown";
                assetDto.Content = element.Content?.TextContent ?? "";
                break;
        }

        return assetDto;
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
                return BadRequest(new { 
                    success = false, 
                    message = "Invalid input data", 
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                });
            }

            _logger.LogInformation("Setting page {PageId} background asset", id);

            var result = await _pageService.SetBackgroundAssetAsync(id, assetDto);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to set page background asset {PageId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(new { 
                    success = false, 
                    message = "Failed to set background asset", 
                    errors = result.Errors.Select(e => e.Message) 
                });
            }
            
            _logger.LogInformation("Page {PageId} background asset set successfully", id);
            return Ok(new { 
                success = true, 
                message = "Background asset set successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting page background asset {PageId}", id);
            return StatusCode(500, new { 
                success = false, 
                message = "Internal server error", 
                error = ex.Message 
            });
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
                return BadRequest(new { 
                    success = false, 
                    message = "Failed to remove background asset", 
                    errors = result.Errors.Select(e => e.Message) 
                });
            }
            
            _logger.LogInformation("Page {PageId} background asset removed successfully", id);
            return Ok(new { 
                success = true, 
                message = "Background asset removed successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing page background asset {PageId}", id);
            return StatusCode(500, new { 
                success = false, 
                message = "Internal server error", 
                error = ex.Message 
            });
        }
    }
}

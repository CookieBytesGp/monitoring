using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using DTOs.Pagebuilder;
using App.Models.PageEditor;
using System.Text.RegularExpressions;

namespace App.Controllers
{
    public class PageEditorController : Controller
    {
        private readonly PageService _pageService;
        private readonly ToolService _toolService;
        private static List<BaseElementDTO> _elements = new();
        // Store the current page’s ID (and title if desired) for final saving.
        private static Guid _currentPageId = Guid.Empty;
        private static string _currentPageTitle = "";

        public PageEditorController(PageService pageService, ToolService toolService)
        {
            _pageService = pageService;
            _toolService = toolService;
        }

        // GET: PageEditor/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var page = await _pageService.GetPageByIdAsync(id);
            if (page == null)
            {
                return NotFound();
            }
            return View(page);
        }

        // POST: PageEditor/Edit/5 (Updating the page and its elements)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, PageDTO pageDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(pageDTO); // Return with validation errors
            }

            var updateResult = await _pageService.UpdatePageAsync(id, pageDTO.Title, pageDTO.Elements);
            if (updateResult.IsFailed)
            {
                ModelState.AddModelError(string.Empty, updateResult.Errors.First().Message);
                return View(pageDTO);
            }

            return RedirectToAction("Index", "Page");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddElement(Guid id, BaseElementDTO elementDTO)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Edit", new { id });
            }

            var result = await _pageService.AddElementAsync(id, elementDTO);
            if (result.IsFailed)
            {
                TempData["ErrorMessage"] = result.Errors.First().Message; // Display the specific error message
            }

            return RedirectToAction("Edit", new { id });
        }

        // POST: RemoveElement—updates _elements when an element is removed
        [HttpPost]
        public IActionResult RemoveElement([FromBody] RemoveElementRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request");
            }

            var element = _elements.FirstOrDefault(e => e.Id == request.ElementId);
            if (element == null)
            {
                return NotFound("Element not found");
            }

            // Remove the specified element.
            _elements.Remove(element);

            // Recalculate order and update defaultCssClasses for each remaining element.
            int newOrder = 1;
            foreach (var elem in _elements)
            {
                // Update the order property
                elem.Order = newOrder;

                // Update each CSS class value to reflect the new order.
                if (elem.TemplateBody?.DefaultCssClasses != null)
                {
                    // Grab a list of keys, so we can iterate and update.
                    var keys = elem.TemplateBody.DefaultCssClasses.Keys.ToList();
                    foreach (var key in keys)
                    {
                        var origVal = elem.TemplateBody.DefaultCssClasses[key];
                        // If the original value is null or empty, use the key as default.
                        string baseValue = string.IsNullOrEmpty(origVal) ? key : origVal;
                        // Remove existing trailing digits using a regex.
                        baseValue = Regex.Replace(baseValue, @"\d+$", "");
                        // Append the new order value.
                        elem.TemplateBody.DefaultCssClasses[key] = baseValue + newOrder.ToString();
                    }
                }

                newOrder++;
            }

            return Ok();
        }


        // GET: PageEditor/EditElements/5
        [HttpGet]
        public async Task<IActionResult> EditElements(Guid id)
        {
            if (_currentPageId != id || !_elements.Any()) // Only fetch from the database if memory is empty or page ID changes
            {
                var pageDTO = await _pageService.GetPageByIdAsync(id);
                if (pageDTO == null)
                {
                    return NotFound();
                }

                _currentPageId = pageDTO.Id;
                _currentPageTitle = pageDTO.Title;
                _elements = pageDTO.Elements ?? new List<BaseElementDTO>();
            }

            // Load tools separately
            var tools = await _toolService.GetToolsAsync();

            var viewModel = new EditElementsViewModel
            {
                PageId = id,
                Tools = tools,
                Elements = _elements
            };

            return View(viewModel);
        }


        // POST: AddToolToEditor—updates _elements when a tool is selected
        [HttpPost]
        public async Task<IActionResult> AddToolToEditor([FromBody] AddToolRequest request)
        {
            if (request == null || request.Tool == null)
            {
                return BadRequest("Tool cannot be null");
            }

            // Retrieve the full tool information from the database.
            var tool = await _toolService.GetToolByIdAsync(request.Tool.Id);
            if (tool == null)
            {
                return NotFound("Tool not found");
            }

            // Use the first available template from the tool's Templates list.
            var firstTemplate = tool.Templates?.FirstOrDefault();
            if (firstTemplate == null)
            {
                return BadRequest("No template defined for the specified tool.");
            }

            // Determine the order for the new element.
            int order = _elements.Count + 1;

            // Process DefaultCssClasses with duplicate-check logic.
            Dictionary<string, string> defaultCssClasses;
            if (firstTemplate.DefaultCssClasses != null)
            {
                defaultCssClasses = firstTemplate.DefaultCssClasses.ToDictionary(
                    kvp => kvp.Key,
                    kvp =>
                    {
                        // Use kvp.Value if it's not empty, otherwise use the key.
                        var baseValue = string.IsNullOrEmpty(kvp.Value) ? kvp.Key : kvp.Value;
                        var orderStr = order.ToString();
                        // If baseValue already ends with the order string, then leave it as is; otherwise, append.
                        return baseValue.EndsWith(orderStr) ? baseValue : baseValue + orderStr;
                    }
                );
            }
            else
            {
                defaultCssClasses = new Dictionary<string, string>
                {
                    { "additionalProp1", "additionalProp1" + order.ToString() }
                };
            }


            // Construct the BaseElementDTO using real data from the tool.
            var baseElement = new BaseElementDTO
            {
                Id = Guid.NewGuid(),
                ToolId = tool.Id,
                Order = order,
                TemplateBody = new TemplateBodyDTO
                {
                    HtmlTemplate = firstTemplate.HtmlTemplate,
                    DefaultCssClasses = defaultCssClasses,
                    CustomCss = firstTemplate.CustomCss ?? "",
                    CustomJs = tool.DefaultJs ?? "",
                    IsFloating = true,
                },
                Asset = tool.DefaultAssets?.FirstOrDefault() ?? new AssetDTO
                {
                    Url = "default-asset-url",
                    Type = "default-type",
                    AltText = "Default Alt",
                    Content = "Default Content",
                    Metadata = new Dictionary<string, string>
            {
                { "additionalProp1", "default" }
            }
                }
            };

            // Add the new base element to the in-memory list.
            _elements.Add(baseElement);

            // Log the current state of the elements list for debugging.
            Console.WriteLine("Current elements list:");
            foreach (var element in _elements)
            {
                Console.WriteLine($"Element ID: {element.Id}, ToolId: {element.ToolId}, Order: {element.Order}");
            }

            return Ok(baseElement);
        }




        [HttpPost]
        public IActionResult SaveElements([FromBody] SaveElementsRequest request)
        {
            if (request == null || request.Elements == null || !request.Elements.Any())
            {
                return BadRequest("Elements list is invalid or empty");
            }

            // Replace the current in-memory elements with the updated ones
            _elements = request.Elements;

            return Ok("Elements updated in the editor");
        }

        // POST: FinalSave—updates the database with the current _elements list
        [HttpPost]
        public async Task<IActionResult> FinalSave()
        {
            if (_currentPageId == Guid.Empty)
            {
                return BadRequest("PageId is missing");
            }
            if (_elements == null || !_elements.Any())
            {
                return BadRequest("Elements list is empty");
            }

            try
            {
                // Get the current title from the database (or from a stored value) if needed.
                var pageDTO = await _pageService.GetPageByIdAsync(_currentPageId);
                if (pageDTO == null)
                {
                    return NotFound("Page not found");
                }

                var result = await _pageService.UpdatePageAsync(_currentPageId, _currentPageTitle, _elements);
                if (result.IsFailed)
                {
                    return BadRequest("Failed to save page: " + string.Join(", ", result.Errors));
                }

                // Clear the in-memory list after successful save.
                _elements.Clear();
                return Ok("Page saved successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal server error occurred: " + ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UpdateElementsList([FromBody] List<BaseElementDTO> updatedElements)
        {
            if (updatedElements == null || !updatedElements.Any())
            {
                return BadRequest("No updated elements provided.");
            }

            // Replace your in-memory list with the updated data.
            _elements = updatedElements;

            int newOrder = 1;
            foreach (var elem in _elements)
            {
                // Reset the order.
                elem.Order = newOrder++;

                // Validate and update the Asset.
                elem.Asset = EnsureAssetIsValid(elem.Asset);
            }

            return Ok("Elements updated successfully.");
        }


        // Helper method that ensures every Asset has valid data.
        private AssetDTO EnsureAssetIsValid(AssetDTO asset)
        {
            // If the asset is null, create a new one with default values.
            if (asset == null)
            {
                return GetDefaultAsset();
            }

            // Validate each property: if null/empty, set default.
            asset.Url = string.IsNullOrWhiteSpace(asset.Url) ? "default-asset-url" : asset.Url;
            asset.Type = string.IsNullOrWhiteSpace(asset.Type) ? "default-type" : asset.Type;
            asset.AltText = string.IsNullOrWhiteSpace(asset.AltText) ? "Default Alt" : asset.AltText;
            asset.Content = string.IsNullOrWhiteSpace(asset.Content) ? "Default Content" : asset.Content;

            // Ensure Metadata is valid.
            if (asset.Metadata == null || !asset.Metadata.Any())
            {
                asset.Metadata = new Dictionary<string, string>
                {
                    { "additionalProp1", "default" }
                };
            }

            return asset;
        }

        // Returns a default AssetDTO with mock data.
        private AssetDTO GetDefaultAsset()
        {
            return new AssetDTO
            {
                Url = "default-asset-url",
                Type = "default-type",
                AltText = "Default Alt",
                Content = "Default Content",
                Metadata = new Dictionary<string, string>
                {
                    { "additionalProp1", "default" }
                }
            };
        }




    }


}

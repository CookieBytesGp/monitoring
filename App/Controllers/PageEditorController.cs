using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using DTOs.Pagebuilder;
using App.Models.PageEditor;

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
        public IActionResult RemoveElement(Guid pageId, Guid elementId)
        {
            var element = _elements.FirstOrDefault(e => e.Id == elementId);
            if (element == null)
            {
                return NotFound("Element not found");
            }

            _elements.Remove(element);

            //// Optionally, update database immediately:
            //var result = await _pageService.RemoveElementAsync(pageId, elementId);
            //if (result.IsFailed)
            //{
            //    return BadRequest("Failed to remove element: " + string.Join(", ", result.Errors));
            //}

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

            // Transform ToolDTO into BaseElementDTO
            var baseElement = new BaseElementDTO
            {
                Id = Guid.NewGuid(),
                ToolId = request.Tool.Id,
                Order = _elements.Count + 1, // Default incremental order
                TemplateBody = new TemplateBodyDTO
                {
                    HtmlTemplate = $"<div>{request.Tool.Name} Template</div>"
                },
                Asset = new AssetDTO
                {
                    Url = "default-asset-url",
                    Type = "default-type",
                    AltText = "Default Alt",
                    Content = "Default Content"
                }
            };

            _elements.Add(baseElement);
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


    }

    // ViewModel for the EditElements view
    public class EditElementsViewModel
    {
        public Guid PageId { get; set; }
        public List<ToolDTO> Tools { get; set; }
        public List<BaseElementDTO> Elements { get; set; }
    }
}

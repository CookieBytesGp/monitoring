using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using DTOs.Pagebuilder;

namespace App.Controllers
{
    public class PageEditorController : Controller
    {
        private readonly PageService _pageService;
        private readonly ToolService _toolService;
        private static List<BaseElementDTO> _elements = new();

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

        // POST: RemoveElement
        [HttpPost]
        public IActionResult RemoveElement([FromBody] Guid elementId)
        {
            var element = _elements.Find(e => e.Id == elementId);
            if (element == null)
            {
                return NotFound("Element not found");
            }

            _elements.Remove(element); // Remove element from the static list

            return Ok();
        }
        // GET: PageEditor/EditElements
        [HttpGet]
        public async Task<IActionResult> EditElements(Guid id)
        {
            var pageDTO = await _pageService.GetPageByIdAsync(id);
            if (pageDTO == null)
            {
                return NotFound();
            }

            // Load tools separately
            var tools = await _toolService.GetToolsAsync(); // Fetch the available tools

            // Pass the page and tools in a ViewModel
            var viewModel = new EditElementsViewModel
            {
                PageId = id,
                Tools = tools,
                Elements = _elements // Use the static list for current elements
            };

            return View(viewModel);
        }
        // POST: AddToolToEditor
        [HttpPost]
        public IActionResult AddToolToEditor([FromBody] ToolDTO tool)
        {
            if (tool == null)
            {
                return BadRequest("Tool cannot be null");
            }

            // Transform ToolDTO into BaseElementDTO
            var baseElement = new BaseElementDTO
            {
                Id = Guid.NewGuid(),
                ToolId = tool.Id,
                Order = _elements.Count + 1, // Default order is incremental
                TemplateBody = new TemplateBodyDTO
                {
                    HtmlTemplate = $"<div>{tool.Name} Template</div>"
                },
                Asset = new AssetDTO
                {
                    Url = "default-asset-url",
                    Type = "default-type",
                    AltText = "Default Alt",
                    Content = "Default Content"
                }
            };

            _elements.Add(baseElement); // Add the element to the static list

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SaveElements([FromBody] SaveElementsRequest request)
        {
            if (request == null)
            {
                Console.WriteLine("Request is null");
                return BadRequest("No data received");
            }
            if (request.PageId == Guid.Empty)
            {
                Console.WriteLine("PageId is missing");
                return BadRequest("PageId is missing");
            }
            if (request.Elements == null || !request.Elements.Any())
            {
                Console.WriteLine("Elements list is empty");
                return BadRequest("Elements list is empty");
            }

            var result = await _pageService.UpdateElementsAsync(request.PageId, request.Elements);
            if (result.IsFailed)
            {
                Console.WriteLine("Failed to update elements: " + string.Join(", ", result.Errors));
                return BadRequest(result.Errors);
            }

            return Ok();
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

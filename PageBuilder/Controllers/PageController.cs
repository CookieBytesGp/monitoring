using DTOs.Pagebuilder;
using Microsoft.AspNetCore.Mvc;
using PageBuilder.Services.PageService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;

namespace PageBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageController : ControllerBase
    {
        private readonly IPageService _pageService;
        private readonly ILogger<PageController> _logger;

        public PageController(IPageService pageService, ILogger<PageController> logger)
        {
            _pageService = pageService;
            _logger = logger;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(Result<PageDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePage([FromBody] PageDTO pageDTO)
        {
            try
            {
                var result = await _pageService.CreatePageAsync(
                    pageDTO.Title,
                    pageDTO.Elements);

                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return CreatedAtAction(nameof(GetPageById), new { id = result.Value.Id }, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating page");
                return StatusCode(500, "An unexpected error occurred while creating the page.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PageDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPageById(Guid id)
        {
            try
            {
                var pageDTO = await _pageService.GetPageAsync(id);
                if (pageDTO == null)
                {
                    return NotFound();
                }

                return Ok(pageDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting page");
                return StatusCode(500, "An unexpected error occurred while getting the page.");
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PageDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPages()
        {
            try
            {
                var result = await _pageService.GetAllAsync();
                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting all pages");
                return StatusCode(500, "An unexpected error occurred while getting all pages.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdatePage(Guid id, [FromBody] PageDTO pageDTO)
        {
            try
            {
                var result = await _pageService.UpdatePageAsync(
                    id,
                    pageDTO.Title,
                    pageDTO.Elements);

                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating page");
                return StatusCode(500, "An unexpected error occurred while updating the page.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeletePage(Guid id)
        {
            try
            {
                var result = await _pageService.DeletePageAsync(id);
                if (result.IsFailed)
                {
                    return NotFound(result.Errors);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting page");
                return StatusCode(500, "An unexpected error occurred while deleting the page.");
            }
        }

        [HttpPost("{id}/element")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddElement(Guid id, [FromBody] BaseElementDTO elementDTO)
        {
            try
            {
                var result = await _pageService.AddElementAsync(id, elementDTO);

                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding element");
                return StatusCode(500, "An unexpected error occurred while adding the element.");
            }
        }

        [HttpPut("{id}/element/{elementId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateElement(Guid id, Guid elementId, [FromBody] BaseElementDTO elementDTO)
        {
            try
            {
                var result = await _pageService.UpdateElementAsync(id, elementId, elementDTO);

                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating element");
                return StatusCode(500, "An unexpected error occurred while updating the element.");
            }
        }

        [HttpDelete("{id}/element/{elementId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveElement(Guid id, Guid elementId)
        {
            try
            {
                var result = await _pageService.RemoveElementAsync(id, elementId);

                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while removing element");
                return StatusCode(500, "An unexpected error occurred while removing the element.");
            }
        }
    }
}

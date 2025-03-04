using DTOs.Pagebuilder;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FluentResults;
using PageBuilder.Services.ToolService;
using Microsoft.Extensions.Logging;

namespace PageBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolController : ControllerBase
    {
        private readonly IToolService _toolService;
        private readonly ILogger<ToolController> _logger;

        public ToolController(IToolService toolService, ILogger<ToolController> logger)
        {
            _toolService = toolService;
            _logger = logger;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(Result<ToolDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateTool([FromBody] ToolDTO toolDTO)
        {
            try
            {
                var result = await _toolService.CreateToolAsync(
                    toolDTO.Name,
                    toolDTO.DefaultJs,
                    toolDTO.ElementType,
                    toolDTO.Templates,
                    toolDTO.DefaultAssets);

                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return CreatedAtAction(nameof(GetToolById), new { id = result.Value.Id }, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating tool");
                return StatusCode(500, "An unexpected error occurred while creating the tool.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ToolDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetToolById(Guid id)
        {
            try
            {
                var toolDTO = await _toolService.GetToolAsync(id);
                if (toolDTO == null)
                {
                    return NotFound();
                }

                return Ok(toolDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting tool");
                return StatusCode(500, "An unexpected error occurred while getting the tool.");
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ToolDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTools()
        {
            try
            {
                var result = await _toolService.GetAllAsync();
                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting all tools");
                return StatusCode(500, "An unexpected error occurred while getting all tools.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateTool(Guid id, [FromBody] ToolDTO toolDTO)
        {
            try
            {
                var result = await _toolService.UpdateToolAsync(
                    id,
                    toolDTO.Name,
                    toolDTO.DefaultJs,
                    toolDTO.ElementType,
                    toolDTO.Templates,
                    toolDTO.DefaultAssets);

                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating tool");
                return StatusCode(500, "An unexpected error occurred while updating the tool.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteTool(Guid id)
        {
            try
            {
                var result = await _toolService.DeleteToolAsync(id);
                if (result.IsFailed)
                {
                    return NotFound(result.Errors);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting tool");
                return StatusCode(500, "An unexpected error occurred while deleting the tool.");
            }
        }
    }
}

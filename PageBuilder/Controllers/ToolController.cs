using Microsoft.AspNetCore.Mvc;
using PageBuilder.Services.ToolService;
using DTOs.Pagebuilder;
using Domain.Aggregates.Tools;
using Domain.Aggregates.Tools.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Domain.SharedKernel.Domain.SharedKernel;

namespace PageBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolController : ControllerBase
    {
        private readonly IToolService _toolService;

        public ToolController(IToolService toolService)
        {
            _toolService = toolService;
        }

        // GET: api/Tool/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetToolById(Guid id)
        {
            var result = await _toolService.GetByIdAsync(id);
            if (result.IsFailed)
            {
                return NotFound(result.Errors);
            }

            var tool = result.Value;
            var toolDTO = new ToolDTO
            {
                Id = tool.Id,
                Name = tool.Name,
                DefaultJs = tool.DefaultJs,
                ElementType = tool.ElementType,
                Templates = tool.Templates.ConvertAll(template => new TemplateDTO
                {
                    HtmlTemplate = template.HtmlStructure,
                    DefaultCssClasses = template.DefaultCssClasses,
                    CustomCss = template.DefaultCss,
                }),
                DefaultAssets = tool.DefaultAssets.ConvertAll(asset => new AssetDTO
                {
                    Url = asset.Url,
                    Type = asset.Type,
                    AltText = asset.AltText,
                    Content = asset.Content,
                    Metadata = asset.Metadata
                })
            };

            return Ok(toolDTO);
        }

        // GET: api/Tool
        [HttpGet]
        public async Task<IActionResult> GetAllTools()
        {
            var result = await _toolService.GetAllAsync();
            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }

            var toolDTOs = result.Value.Select(tool => new ToolDTO
            {
                Id = tool.Id,
                Name = tool.Name,
                DefaultJs = tool.DefaultJs,
                ElementType = tool.ElementType,
                Templates = tool.Templates.ConvertAll(template => new TemplateDTO
                {
                    HtmlTemplate = template.HtmlStructure,
                    DefaultCssClasses = template.DefaultCssClasses,
                    CustomCss = template.DefaultCss
                }),
                DefaultAssets = tool.DefaultAssets.ConvertAll(asset => new AssetDTO
                {
                    Url = asset.Url,
                    Type = asset.Type,
                    AltText = asset.AltText,
                    Content = asset.Content,
                    Metadata = asset.Metadata
                })
            }).ToList();

            return Ok(toolDTOs);
        }

        // POST: api/Tool
        [HttpPost]
        public async Task<IActionResult> CreateTool([FromBody] ToolDTO toolDTO)
        {
            var templateResults = toolDTO.Templates.Select(templateDTO =>
                Template.Create(
                    templateDTO.HtmlTemplate,
                    templateDTO.DefaultCssClasses,
                    templateDTO.CustomCss
                )).ToList();

            var templateErrors = templateResults
                .Where(r => r.IsFailed)
                .SelectMany(r => r.Errors)
                .ToList();

            var assetResults = toolDTO.DefaultAssets.Select(assetDTO =>
                Asset.Create(
                    assetDTO.Url,
                    assetDTO.Type,
                    assetDTO.Content,
                    assetDTO.AltText,
                    assetDTO.Metadata
                )).ToList();

            var assetErrors = assetResults
                .Where(r => r.IsFailed)
                .SelectMany(r => r.Errors)
                .ToList();

            var allErrors = templateErrors.Concat(assetErrors).ToList();

            if (allErrors.Any())
            {
                return BadRequest(allErrors.Select(e => e.Message));
            }

            var templates = templateResults.Where(r => r.IsSuccess).Select(r => r.Value).ToList();
            var defaultAssets = assetResults.Where(r => r.IsSuccess).Select(r => r.Value).ToList();

            var result = await _toolService.CreateAsync(
                toolDTO.Name,
                toolDTO.DefaultJs,
                toolDTO.ElementType,
                templates,
                defaultAssets
            );

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return CreatedAtAction(nameof(GetToolById), new { id = result.Value.Id }, toolDTO);
        }

            // DELETE: api/Tool/{id}
            [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTool(Guid id)
        {
            var result = await _toolService.DeleteAsync(id);
            if (result.IsFailed)
            {
                return NotFound(result.Errors.Select(e => e.Message));
            }

            return NoContent();
        }
        // PUT: api/Tool/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTool(Guid id, [FromBody] ToolDTO toolDTO)
        {
            // Create Templates
            var templateResults = toolDTO.Templates.Select(templateDTO =>
                Template.Create(
                    templateDTO.HtmlTemplate,
                    templateDTO.DefaultCssClasses,
                    templateDTO.CustomCss
                )).ToList();

            // Collect any template creation errors
            var templateErrors = templateResults
                .Where(r => r.IsFailed)
                .SelectMany(r => r.Errors)
                .ToList();

            // Create Assets
            var assetResults = toolDTO.DefaultAssets.Select(assetDTO =>
                Asset.Create(
                    assetDTO.Url,
                    assetDTO.Type,
                    assetDTO.Content,
                    assetDTO.AltText,
                    assetDTO.Metadata
                )).ToList();

            // Collect any asset creation errors
            var assetErrors = assetResults
                .Where(r => r.IsFailed)
                .SelectMany(r => r.Errors)
                .ToList();

            // Combine all errors
            var allErrors = templateErrors.Concat(assetErrors).ToList();

            if (allErrors.Any())
            {
                return BadRequest(allErrors.Select(e => e.Message));
            }

            // Extract the successfully created Templates and Assets
            var templates = templateResults.Select(r => r.Value).ToList();
            var defaultAssets = assetResults.Select(r => r.Value).ToList();

            // Call the service to update the Tool
            var result = await _toolService.UpdateAsync(
                id,
                toolDTO.Name,
                toolDTO.DefaultJs,
                toolDTO.ElementType,
                templates,
                defaultAssets
            );

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return NoContent();
        }

    }
}


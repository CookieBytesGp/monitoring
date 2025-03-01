using Microsoft.AspNetCore.Mvc;
using PageBuilder.Services.PageService;
using Domain.Aggregates.Page;
using Domain.Aggregates.Page.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using DTOs.Pagebuilder;
using Domain.SharedKernel.Domain.SharedKernel;

namespace PageBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageController : ControllerBase
    {
        private readonly IPageService _pageService;

        public PageController(IPageService pageService)
        {
            _pageService = pageService;
        }

        // GET: api/Page/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPageById(Guid id)
        {
            var result = await _pageService.GetByIdAsync(id);
            if (result.IsFailed)
            {
                return NotFound(result.Errors);
            }

            var page = result.Value;
            var pageDTO = new PageDTO
            {
                Id = page.Id,
                Title = page.Title,
                CreatedAt = page.CreatedAt,
                UpdatedAt = page.UpdatedAt,
                Elements = page.Elements.Select(element => new BaseElementDTO
                {
                    Id = element.Id,
                    ToolId = element.ToolId,
                    Order = element.Order,
                    TemplateBody = new TemplateBodyDTO
                    {
                        HtmlTemplate = element.TemplateBody.HtmlTemplate,
                        DefaultCssClasses = element.TemplateBody.DefaultCssClasses,
                        CustomCss = element.TemplateBody.CustomCss,
                        CustomJs = element.TemplateBody.CustomJs,
                        IsFloating = element.TemplateBody.IsFloating
                    },
                    Asset = new AssetDTO
                    {
                        Url = element.Asset.Url,
                        Type = element.Asset.Type,
                        Content = element.Asset.Content,
                        AltText = element.Asset.AltText,
                        Metadata = element.Asset.Metadata
                    }
                }).ToList()
            };

            return Ok(pageDTO);
        }

        // GET: api/Page
        [HttpGet]
        public async Task<IActionResult> GetAllPages()
        {
            var result = await _pageService.GetAllAsync();
            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }

            var pageDTOs = result.Value.Select(page => new PageDTO
            {
                Id = page.Id,
                Title = page.Title,
                CreatedAt = page.CreatedAt,
                UpdatedAt = page.UpdatedAt,
                Elements = page.Elements.Select(element => new BaseElementDTO
                {
                    Id = element.Id,
                    ToolId = element.ToolId,
                    Order = element.Order,
                    TemplateBody = new TemplateBodyDTO
                    {
                        HtmlTemplate = element.TemplateBody.HtmlTemplate,
                        DefaultCssClasses = element.TemplateBody.DefaultCssClasses,
                        CustomCss = element.TemplateBody.CustomCss,
                        CustomJs = element.TemplateBody.CustomJs,
                        IsFloating = element.TemplateBody.IsFloating
                    },
                    Asset = new AssetDTO
                    {
                        Url = element.Asset.Url,
                        Type = element.Asset.Type,
                        Content = element.Asset.Content,
                        AltText = element.Asset.AltText,
                        Metadata = element.Asset.Metadata
                    }
                }).ToList()
            }).ToList();

            return Ok(pageDTOs);
        }

        // POST: api/Page
        [HttpPost]
        public async Task<IActionResult> CreatePage([FromBody] PageDTO pageDTO)
        {
            var result = await _pageService.CreateAsync(pageDTO.Title);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return CreatedAtAction(nameof(GetPageById), new { id = result.Value.Id }, pageDTO);
        }

        // PUT: api/Page/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePage(Guid id, [FromBody] PageDTO pageDTO)
        {
            var result = await _pageService.UpdateAsync(id, pageDTO.Title);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return NoContent();
        }

        // DELETE: api/Page/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePage(Guid id)
        {
            var result = await _pageService.DeleteAsync(id);
            if (result.IsFailed)
            {
                return NotFound(result.Errors.Select(e => e.Message));
            }

            return NoContent();
        }

        // POST: api/Page/{id}/element
        [HttpPost("{id}/element")]
        public async Task<IActionResult> AddElement(Guid id, [FromBody] BaseElementDTO elementDTO)
        {
            var templateBodyResult = TemplateBody.Create(
                elementDTO.TemplateBody.HtmlTemplate,
                elementDTO.TemplateBody.DefaultCssClasses,
                elementDTO.TemplateBody.CustomCss,
                elementDTO.TemplateBody.CustomJs,
                elementDTO.TemplateBody.IsFloating
            );

            if (templateBodyResult.IsFailed)
            {
                return BadRequest(templateBodyResult.Errors.Select(e => e.Message));
            }

            var assetResult = Asset.Create(
                elementDTO.Asset.Url,
                elementDTO.Asset.Type,
                elementDTO.Asset.Content,
                elementDTO.Asset.AltText,
                elementDTO.Asset.Metadata
            );

            if (assetResult.IsFailed)
            {
                return BadRequest(assetResult.Errors.Select(e => e.Message));
            }

            var elementResult = BaseElement.Create(
                elementDTO.ToolId,
                elementDTO.Order,
                templateBodyResult.Value,
                assetResult.Value
            );

            if (elementResult.IsFailed)
            {
                return BadRequest(elementResult.Errors.Select(e => e.Message));
            }

            var element = elementResult.Value;

            var result = await _pageService.AddElementAsync(id, element);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return NoContent();
        }

        // DELETE: api/Page/{id}/element/{elementId}
        [HttpDelete("{id}/element/{elementId}")]
        public async Task<IActionResult> RemoveElement(Guid id, Guid elementId)
        {
            var pageResult = await _pageService.GetByIdAsync(id);
            if (pageResult.IsFailed)
            {
                return NotFound(pageResult.Errors.Select(e => e.Message));
            }

            var element = pageResult.Value.Elements.FirstOrDefault(e => e.Id == elementId);

            if (element == null)
            {
                return NotFound("Element not found.");
            }

            var result = await _pageService.RemoveElementAsync(id, element);

            if (result.IsFailed)
            {
                return BadRequest(result.Errors.Select(e => e.Message));
            }

            return NoContent();
        }
    }
}

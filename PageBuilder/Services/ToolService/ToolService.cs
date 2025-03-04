using Domain.Aggregates.Tools;
using Domain.Aggregates.Tools.ValueObjects;
using Domain.SharedKernel.Domain.SharedKernel;
using DTOs.Pagebuilder;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PageBuilder.Services.ToolService
{
    public class ToolService : IToolService
    {
        private readonly Persistence.IUnitOfWork _unitOfWork;

        public ToolService(Persistence.IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ToolDTO>> CreateToolAsync(string name, string defaultJs, string elementType, List<TemplateDTO> templates, List<AssetDTO> defaultAssets)
        {
            var toolResult = Tool.Create(name, defaultJs, elementType, ConvertToDomainTemplates(templates), ConvertToDomainAssets(defaultAssets));

            if (toolResult.IsFailed)
            {
                return Result.Fail<ToolDTO>(toolResult.Errors);
            }

            await _unitOfWork.ToolRepository.AddAsync(toolResult.Value);
            await _unitOfWork.SaveAsync();

            var toolDTO = new ToolDTO
            {
                Id = toolResult.Value.Id,
                Name = toolResult.Value.Name,
                DefaultJs = toolResult.Value.DefaultJs,
                ElementType = toolResult.Value.ElementType,
                Templates = templates,
                DefaultAssets = defaultAssets
            };

            return Result.Ok(toolDTO);
        }

        public async Task<ToolDTO> GetToolAsync(Guid id)
        {
            var tool = await GetToolFromDatabaseAsync(id);
            if (tool == null)
            {
                return null;
            }

            var toolDTO = new ToolDTO
            {
                Id = tool.Id,
                Name = tool.Name,
                DefaultJs = tool.DefaultJs,
                ElementType = tool.ElementType,
                Templates = ConvertToDTOs(tool.Templates),
                DefaultAssets = ConvertToDTOs(tool.DefaultAssets)
            };

            return toolDTO;
        }

        private async Task<Tool> GetToolFromDatabaseAsync(Guid id)
        {
            var tool = await _unitOfWork.ToolRepository.FindAsync(id);
            return tool;
        }
        public async Task<Result<IEnumerable<ToolDTO>>> GetAllAsync()
        {
            var tools = await _unitOfWork.ToolRepository.GetAllAsync();

            var toolDTOs = tools.Select(tool => new ToolDTO
            {
                Id = tool.Id,
                Name = tool.Name,
                DefaultJs = tool.DefaultJs,
                ElementType = tool.ElementType,
                Templates = ConvertToDTOs(tool.Templates),
                DefaultAssets = ConvertToDTOs(tool.DefaultAssets)
            }).ToList();

            return Result.Ok((IEnumerable<ToolDTO>)toolDTOs); // Cast the List to IEnumerable
        }

        public async Task<Result> UpdateToolAsync(Guid id, string name, string defaultJs, string elementType, List<TemplateDTO> templates, List<AssetDTO> defaultAssets)
        {
            var tool = await GetToolFromDatabaseAsync(id);

            if (tool == null)
            {
                return Result.Fail("Tool not found.");
            }

            tool.Update(name, defaultJs, elementType, ConvertToDomainTemplates(templates), ConvertToDomainAssets(defaultAssets));
            await _unitOfWork.ToolRepository.UpdateAsync(tool);
            await _unitOfWork.SaveAsync();

            return Result.Ok();
        }

        public async Task<Result> DeleteToolAsync(Guid id)
        {
            var success = await _unitOfWork.ToolRepository.RemoveByIdAsync(id);
            if (!success)
            {
                return Result.Fail("Tool not found.");
            }

            await _unitOfWork.SaveAsync();
            return Result.Ok();
        }

        private List<Template> ConvertToDomainTemplates(List<TemplateDTO> templateDTOs)
        {
            return templateDTOs.Select(dto => Template.Create(dto.HtmlTemplate, dto.DefaultCssClasses, dto.CustomCss).Value).ToList();
        }

        private List<Asset> ConvertToDomainAssets(List<AssetDTO> assetDTOs)
        {
            return assetDTOs.Select(dto => Asset.Create(dto.Url, dto.Type, dto.Content, dto.AltText, dto.Metadata).Value).ToList();
        }

        private List<TemplateDTO> ConvertToDTOs(List<Template> templates)
        {
            return templates.Select(template => new TemplateDTO
            {
                HtmlTemplate = template.HtmlStructure,
                DefaultCssClasses = template.DefaultCssClasses,
                CustomCss = template.DefaultCss
            }).ToList();
        }

        private List<AssetDTO> ConvertToDTOs(List<Asset> assets)
        {
            return assets.Select(asset => new AssetDTO
            {
                Url = asset.Url,
                Type = asset.Type,
                AltText = asset.AltText,
                Content = asset.Content,
                Metadata = asset.Metadata
            }).ToList();
        }
    }
}

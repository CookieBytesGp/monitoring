
using Monitoring.Application.Interfaces.Tool;
using Monitoring.Common.Interfaces;
using Monitoring.Infrastructure.Persistence;
using Monitoring.Application.DTOs.Tool;
using FluentResults;
using Monitoring.Domain.Aggregates.Tools;
using Domain.Aggregates.Tools.ValueObjects;
using Domain.SharedKernel;

namespace Monitoring.Application.Services.Tool
{
    public class ToolService : IToolService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ToolService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<ToolDTO>>> GetAllToolsAsync()
        {
            try
            {
                var tools = await _unitOfWork.ToolRepository.GetAllAsync();
                var toolDTOs = tools.Select(MapToDTO).ToList();
                return Result.Ok(toolDTOs);
            }
            catch (Exception ex)
            {
                return Result.Fail($"خطا در دریافت ابزارها: {ex.Message}");
            }
        }

        public async Task<Result<ToolDTO>> GetToolByIdAsync(Guid id)
        {
            try
            {
                var tool = await _unitOfWork.ToolRepository.FindAsync(id);
                if (tool == null)
                {
                    return Result.Fail("ابزار مورد نظر یافت نشد");
                }

                var toolDTO = MapToDTO(tool);
                return Result.Ok(toolDTO);
            }
            catch (Exception ex)
            {
                return Result.Fail($"خطا در دریافت ابزار: {ex.Message}");
            }
        }

        public async Task<Result<List<ToolDTO>>> GetToolsByElementTypeAsync(string elementType)
        {
            try
            {
                var tools = await _unitOfWork.ToolRepository.GetByElementTypeAsync(elementType);
                var toolDTOs = tools.Select(MapToDTO).ToList();
                return Result.Ok(toolDTOs);
            }
            catch (Exception ex)
            {
                return Result.Fail($"خطا در دریافت ابزارها: {ex.Message}");
            }
        }

        #region Private Methods

        private ToolDTO MapToDTO(Monitoring.Domain.Aggregates.Tools.Tool tool)
        {
            return new ToolDTO
            {
                Id = tool.Id,
                Name = tool.Name,
                ElementType = tool.ElementType,
                Description = string.Empty, // Tool entity doesn't have description
                Category = string.Empty, // Tool entity doesn't have category  
                IconClass = string.Empty, // Tool entity doesn't have icon class
                IsActive = true, // Default value
                Order = 0, // Default value
                ConfigSchema = string.Empty, // Tool entity doesn't have config schema
                Templates = tool.Templates?.Select(MapTemplateToDTO).ToList() ?? new List<TemplateDTO>(),
                DefaultAssets = tool.DefaultAssets?.Select(MapAssetToDTO).ToList() ?? new List<AssetDTO>(),
                CreatedAt = tool.CreatedAt,
                UpdatedAt = tool.UpdatedAt ?? DateTime.UtcNow
            };
        }

        private TemplateDTO MapTemplateToDTO(Template template)
        {
            return new TemplateDTO
            {
                Id = Guid.NewGuid(), // Template is a value object, no Id
                Name = "Default Template", // Template doesn't have name
                HtmlTemplate = template.HtmlStructure,
                CustomCss = template.DefaultCss,
                CustomJs = string.Empty, // Template doesn't have JS
                IsDefault = true, // Default value
                PreviewImageUrl = string.Empty, // Template doesn't have preview image
                ConfigSchema = string.Empty // Template doesn't have config schema
            };
        }

        private AssetDTO MapAssetToDTO(Asset asset)
        {
            return new AssetDTO
            {
                Id = Guid.NewGuid(), // Asset is a value object, no Id
                Url = asset.Url ?? string.Empty,
                Type = asset.Type,
                AltText = asset.AltText ?? string.Empty,
                Content = asset.Content ?? string.Empty,
                Size = 0 // Asset doesn't have size property
            };
        }

        #endregion
    }
}

using DTOs.Pagebuilder;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PageBuilder.Services.ToolService
{
    public interface IToolService
    {
        Task<Result<ToolDTO>> CreateToolAsync(string name, string defaultJs, string elementType, List<TemplateDTO> templates, List<AssetDTO> defaultAssets);
        Task<ToolDTO> GetToolAsync(Guid id);
        Task<Result> UpdateToolAsync(Guid id, string name, string defaultJs, string elementType, List<TemplateDTO> templates, List<AssetDTO> defaultAssets);
        Task<Result> DeleteToolAsync(Guid id);
        Task<Result<IEnumerable<ToolDTO>>> GetAllAsync();
    }
}

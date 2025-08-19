using FluentResults;
using Monitoring.Application.DTOs.Tool;

namespace Monitoring.Application.Interfaces.Tool
{
    public interface IToolService
    {
        Task<Result<List<ToolDTO>>> GetAllToolsAsync();
        Task<Result<ToolDTO>> GetToolByIdAsync(Guid id);
        Task<Result<List<ToolDTO>>> GetToolsByElementTypeAsync(string elementType);
    }
}

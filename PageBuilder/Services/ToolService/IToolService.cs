using Domain.Aggregates.Tools.ValueObjects;
using Domain.Aggregates.Tools;
using Domain.SharedKernel.Domain.SharedKernel;
using FluentResults;

namespace PageBuilder.Services.ToolService
{
    public interface IToolService
    {
        Task<Result<Tool>> GetByIdAsync(Guid id);
        Task<Result<IEnumerable<Tool>>> GetAllAsync();
        Task<Result> CreateAsync(string name, string defaultJs, string elementType, List<Template> templates, List<Asset> defaultAssets);
        Task<Result> UpdateAsync(Guid id, string name, string defaultJs, string elementType, List<Template> templates, List<Asset> defaultAssets);
        Task<Result> DeleteAsync(Guid id);
    }
}

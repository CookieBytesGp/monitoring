using FluentResults;
using Domain.Aggregates.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IToolService
    {
        Task<Result<>> GetByIdAsync(Guid id);
        Task<Result<IEnumerable<Tool>>> GetAllAsync();
        Task<Result> CreateAsync(string name, string defaultJs, string elementType, List<Template> templates, List<Asset> defaultAssets);
        Task<Result> UpdateAsync(Guid id, string name, string defaultJs, string elementType, List<Template> templates, List<Asset> defaultAssets);
        Task<Result> DeleteAsync(Guid id);
    }
}

using Domain.Aggregates.Tools;
using Domain.Aggregates.Tools.ValueObjects;
using Domain.SharedKernel.Domain.SharedKernel;
using FluentResults;
using Persistence.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PageBuilder.Services.ToolService
{
    public class ToolService : IToolService
    {
        private readonly IToolRepository _toolRepository;

        public ToolService(IToolRepository toolRepository)
        {
            _toolRepository = toolRepository;
        }

        public async Task<Result<Tool>> GetByIdAsync(Guid id)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null)
            {
                return Result.Fail<Tool>("Tool not found.");
            }
            return Result.Ok(tool);
        }

        public async Task<Result<IEnumerable<Tool>>> GetAllAsync()
        {
            var tools = await _toolRepository.GetAllAsync();
            return Result.Ok(tools);
        }

        public async Task<Result> CreateAsync(string name, string defaultJs, string elementType, List<Template> templates, List<Asset> defaultAssets)
        {
            var toolResult = Tool.Create(name, defaultJs, elementType, templates, defaultAssets);

            if (toolResult.IsFailed)
            {
                return Result.Fail(toolResult.Errors);
            }

            await _toolRepository.AddAsync(toolResult.Value);
            return Result.Ok();
        }

        public async Task<Result> UpdateAsync(Guid id, string name, string defaultJs, string elementType, List<Template> templates, List<Asset> defaultAssets)
        {
            var existingTool = await _toolRepository.GetByIdAsync(id);

            if (existingTool == null)
            {
                return Result.Fail("Tool not found.");
            }

            var updatedToolResult = Tool.Create(name, defaultJs, elementType, templates, defaultAssets);

            if (updatedToolResult.IsFailed)
            {
                return Result.Fail(updatedToolResult.Errors);
            }

            _toolRepository.UpdateAsync(updatedToolResult.Value);
            return Result.Ok();
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var tool = await _toolRepository.GetByIdAsync(id);

            if (tool == null)
            {
                return Result.Fail("Tool not found.");
            }

            await _toolRepository.DeleteAsync(tool.Id);
            return Result.Ok();
        }
    }
}

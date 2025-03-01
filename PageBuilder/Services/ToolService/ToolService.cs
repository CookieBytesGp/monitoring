using FluentResults;
using Persistence.Tools;
using Domain.Aggregates.Tools;
using Domain.Aggregates.Tools.ValueObjects;
using Domain.SharedKernel.Domain.SharedKernel;


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

        public async Task<Result<List<Tool>>> GetAllAsync()
        {
            var tools = await _toolRepository.GetAllAsync();
            return Result.Ok(tools.ToList());
        }

        public async Task<Result<Tool>> CreateAsync(string name, string defaultJs, string elementType, List<Template> templates, List<Asset> defaultAssets)
        {
            var toolResult = Tool.Create(name, defaultJs, elementType, templates, defaultAssets);

            if (toolResult.IsFailed)
            {
                return Result.Fail<Tool>(toolResult.Errors);
            }

            await _toolRepository.AddAsync(toolResult.Value);
            return Result.Ok(toolResult.Value);
        }

        public async Task<Result> UpdateAsync(Guid id, string name, string defaultJs, string elementType, List<Template> templates, List<Asset> defaultAssets)
        {
            var tool = await _toolRepository.GetByIdAsync(id);

            if (tool == null)
            {
                return Result.Fail("Tool not found.");
            }

            tool.Update(name, defaultJs, elementType, templates, defaultAssets); // Ensure Update method handles properties

            await _toolRepository.UpdateAsync(tool);

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


        public async Task<Result<Template>> CreateTemplateAsync(string htmlTemplate, Dictionary<string, string> defaultCssClasses, string customCss)
        {
            return Template.Create(htmlTemplate, defaultCssClasses, customCss);
        }

        public async Task<Result<Asset>> CreateAssetAsync(string url, string type, string content, string altText, Dictionary<string, string> metadata)
        {
            return Asset.Create(url, type, content, altText, metadata);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DTOs.Pagebuilder;
using FluentResults;

namespace App.Services
{
    public class PageService
    {
        private readonly HttpClient _httpClient;

        public PageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PageDTO>> GetPagesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<PageDTO>>("http://localhost:5000/pagebuilder/page");
            return response ?? new List<PageDTO>();
        }

        public async Task<PageDTO> GetPageByIdAsync(Guid id)
        {
            var response = await _httpClient.GetFromJsonAsync<PageDTO>($"http://localhost:5000/pagebuilder/page/{id}");
            return response;
        }

        public async Task CreatePageAsync(PageDTO pageDTO)
        {
            var response = await _httpClient.PostAsJsonAsync("http://localhost:5000/pagebuilder/page/create", pageDTO);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Result> UpdatePageAsync(Guid id, string title, List<BaseElementDTO> elements)
        {
            try
            {
                // Fetch the existing page to retain immutable fields (e.g., CreatedAt).
                var existingPage = await GetPageByIdAsync(id);
                if (existingPage == null)
                {
                    return Result.Fail("Page not found");
                }

                // Build a new PageDTO with non-null properties.
                var updatedPage = new PageDTO
                {
                    Id = id,
                    Title = title,
                    CreatedAt = existingPage.CreatedAt, // Preserve the original creation time.
                    UpdatedAt = DateTime.UtcNow,        // Update the timestamp for the modification.
                    Elements = elements.Select(e => new BaseElementDTO
                    {
                        Id = e.Id,
                        ToolId = e.ToolId,
                        Order = e.Order,
                        TemplateBody = new TemplateBodyDTO
                        {
                            HtmlTemplate = e.TemplateBody.HtmlTemplate ?? "<div>Default Template</div>",
                            DefaultCssClasses = e.TemplateBody.DefaultCssClasses ?? new Dictionary<string, string>
                    {
                        { "additionalProp1", "default" },
                        { "additionalProp2", "default" }
                    },
                            CustomCss = e.TemplateBody.CustomCss ?? "",
                            CustomJs = e.TemplateBody.CustomJs ?? "",
                            IsFloating = e.TemplateBody.IsFloating
                        },
                        Asset = new AssetDTO
                        {
                            Url = e.Asset.Url ?? "default-url",
                            Type = e.Asset.Type ?? "default-type",
                            AltText = e.Asset.AltText ?? "default-alt",
                            Content = e.Asset.Content ?? "default-content",
                            Metadata = e.Asset.Metadata ?? new Dictionary<string, string>
                    {
                        { "additionalProp1", "default" },
                        { "additionalProp2", "default" }
                    }
                        }
                    }).ToList()
                };

                // Send the updated page to the API using an HTTP PUT request.
                var content = JsonContent.Create(updatedPage);
                var response = await _httpClient.PutAsync($"http://localhost:5000/pagebuilder/page/{id}", content);

                // Check if the response indicates a failure.
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return Result.Fail($"Failed to update the page. Server returned status code: {response.StatusCode}. Error: {errorMessage}");
                }

                return Result.Ok(); // Return success if the operation was successful.
            }
            catch (HttpRequestException ex)
            {
                // Handle network-related exceptions.
                return Result.Fail($"An HTTP request error occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions.
                return Result.Fail($"An unexpected error occurred while updating the page: {ex.Message}");
            }
        }


        public async Task DeletePageAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"http://localhost:5000/pagebuilder/page/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<Result> AddElementAsync(Guid pageId, BaseElementDTO elementDTO)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"http://localhost:5000/pagebuilder/page/{pageId}/element", elementDTO);
                if (!response.IsSuccessStatusCode)
                {
                    return Result.Fail($"Failed to add element. Server returned status code: {response.StatusCode}");
                }

                return Result.Ok();
            }
            catch (HttpRequestException ex)
            {
                return Result.Fail($"An error occurred while adding the element: {ex.Message}");
            }
        }

        public async Task<Result> RemoveElementAsync(Guid pageId, Guid elementId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"http://localhost:5000/pagebuilder/page/{pageId}/element/{elementId}");
                if (!response.IsSuccessStatusCode)
                {
                    return Result.Fail($"Failed to remove element. Server returned status code: {response.StatusCode}");
                }

                return Result.Ok();
            }
            catch (HttpRequestException ex)
            {
                return Result.Fail($"An error occurred while removing the element: {ex.Message}");
            }
        }


        public async Task<Result> UpdateElementsAsync(Guid pageId, List<BaseElementDTO> elements)
        {
            var errors = new List<string>();
            foreach (var element in elements)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync(
                        $"http://localhost:5000/pagebuilder/page/{pageId}/element/{element.Id}",
                        element);
                    if (!response.IsSuccessStatusCode)
                    {
                        errors.Add($"Element {element.Id} update failed with status code {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Element {element.Id} update threw exception: {ex.Message}");
                }
            }

            if (errors.Any())
            {
                return Result.Fail(string.Join("; ", errors));
            }
            return Result.Ok();
        }

    }
}

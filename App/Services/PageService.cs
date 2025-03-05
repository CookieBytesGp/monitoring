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
            var pageDTO = new PageDTO
            {
                Id = id,
                Title = title,
                Elements = elements
            };

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"http://localhost:5000/pagebuilder/page/{id}", pageDTO);
                if (!response.IsSuccessStatusCode)
                {
                    return Result.Fail($"Failed to update the page. Server returned status code: {response.StatusCode}");
                }

                return Result.Ok();
            }
            catch (HttpRequestException ex)
            {
                return Result.Fail($"An error occurred while updating the page: {ex.Message}");
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

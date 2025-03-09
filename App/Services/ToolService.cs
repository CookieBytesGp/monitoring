using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DTOs.Pagebuilder;

namespace App.Services
{
    public class ToolService
    {
        private readonly HttpClient _httpClient;

        public ToolService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ToolDTO>> GetToolsAsync()
        {
            var response = await _httpClient.GetAsync("http://localhost:5000/pagebuilder/tool");

            if (response.IsSuccessStatusCode)
            {
                var tools = await response.Content.ReadFromJsonAsync<List<ToolDTO>>();
                return tools ?? new List<ToolDTO>();
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorMessage}");
            }
        }
        public async Task<ToolDTO> GetToolByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"http://localhost:5000/pagebuilder/tool/{id}");

            if (response.IsSuccessStatusCode)
            {
                var tool = await response.Content.ReadFromJsonAsync<ToolDTO>();
                return tool;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request for tool id {id} failed with status code {response.StatusCode}: {errorMessage}");
            }
        }
    }
}

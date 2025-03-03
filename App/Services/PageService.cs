using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DTOs.Pagebuilder;

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

        public async Task DeletePageAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"http://localhost:5000/pagebuilder/page/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}

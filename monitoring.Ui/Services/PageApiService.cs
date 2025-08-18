using Monitoring.Ui.Models.Page;
using Monitoring.Ui.Interfaces;
using System.Text.Json;
using System.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Monitoring.Ui.Services
{
    public class PageApiService : IPageApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PageApiService> _logger;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public PageApiService(HttpClient httpClient, ILogger<PageApiService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // Base address should point to API Gateway
            var baseUrl = configuration["ApiGateway:BaseUrl"] ?? "http://localhost:7001";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<List<PageViewModel>> GetAllPagesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/page");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var pages = JsonSerializer.Deserialize<List<PageViewModel>>(json, _jsonOptions);
                    return pages ?? new List<PageViewModel>();
                }
                
                _logger.LogWarning("Failed to get pages from API Gateway. Status: {StatusCode}", response.StatusCode);
                return new List<PageViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pages from API Gateway");
                return new List<PageViewModel>();
            }
        }

        public async Task<PageViewModel> GetPageByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/page/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PageViewModel>(json, _jsonOptions);
                }
                
                _logger.LogWarning("Failed to get page {PageId} from API Gateway. Status: {StatusCode}", id, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting page {PageId} from API Gateway", id);
                return null;
            }
        }

        public async Task<PageViewModel> CreatePageAsync(CreatePageRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/page", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PageViewModel>(responseJson, _jsonOptions);
                }

                _logger.LogWarning("Failed to create page. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating page via API Gateway");
                return null;
            }
        }

        public async Task<bool> UpdatePageAsync(Guid id, UpdatePageRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/page/{id}", content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                _logger.LogWarning("Failed to update page {PageId}. Status: {StatusCode}", id, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {PageId} via API Gateway", id);
                return false;
            }
        }

        public async Task<bool> UpdateDisplaySizeAsync(Guid id, UpdateDisplaySizeRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/page/{id}/display-size", content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                _logger.LogWarning("Failed to update page {PageId} display size. Status: {StatusCode}", id, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {PageId} display size via API Gateway", id);
                return false;
            }
        }

        public async Task<bool> UpdateStatusAsync(Guid id, UpdateStatusRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/page/{id}/status", content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                _logger.LogWarning("Failed to update page {PageId} status. Status: {StatusCode}", id, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {PageId} status via API Gateway", id);
                return false;
            }
        }

        public async Task<bool> UpdateThumbnailAsync(Guid id, UpdateThumbnailRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/page/{id}/thumbnail", content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                _logger.LogWarning("Failed to update page {PageId} thumbnail. Status: {StatusCode}", id, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {PageId} thumbnail via API Gateway", id);
                return false;
            }
        }

        public async Task<bool> DeletePageAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/page/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                _logger.LogWarning("Failed to delete page {PageId} from API Gateway. Status: {StatusCode}", id, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting page {PageId} via API Gateway", id);
                return false;
            }
        }
    }
}

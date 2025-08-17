using DTOs.Pagebuilder;
using Monitoring.Application.DTOs.Page;
using System.Text.Json;
using System.Text;
using Domain.Aggregates.Page.ValueObjects;

namespace Monitoring.Ui.Services
{
    public interface IPageApiService
    {
        Task<IEnumerable<PageDTO>> GetAllPagesAsync();
        Task<PageDTO> GetPageByIdAsync(Guid id);
        Task<PageDTO> CreatePageAsync(string title, int displayWidth, int displayHeight, string orientation);
        Task<bool> UpdatePageAsync(Guid id, string title, int displayWidth, int displayHeight, string orientation, string thumbnailUrl);
        Task<bool> DeletePageAsync(Guid id);
        Task<bool> TogglePageStatusAsync(Guid id, string status);
        Task<bool> SetPageThumbnailAsync(Guid id, string thumbnailUrl);
    }

    public class PageApiService : IPageApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PageApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public PageApiService(HttpClient httpClient, ILogger<PageApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // Base address should point to API Gateway
            _httpClient.BaseAddress = new Uri("https://localhost:5001/"); // API Gateway address
        }

        public async Task<IEnumerable<PageDTO>> GetAllPagesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/page");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var pages = JsonSerializer.Deserialize<IEnumerable<PageDTO>>(json, _jsonOptions);
                    return pages ?? new List<PageDTO>();
                }
                
                _logger.LogWarning("Failed to get pages from API Gateway. Status: {StatusCode}", response.StatusCode);
                return new List<PageDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pages from API Gateway");
                return new List<PageDTO>();
            }
        }

        public async Task<PageDTO> GetPageByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/page/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PageDTO>(json, _jsonOptions);
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

        public async Task<PageDTO> CreatePageAsync(string title, int displayWidth, int displayHeight, string orientation)
        {
            try
            {
                var request = new
                {
                    Title = title,
                    DisplayWidth = displayWidth,
                    DisplayHeight = displayHeight,
                    Orientation = orientation,
                    Elements = new List<object>() // Empty elements list
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/page", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PageDTO>(responseJson, _jsonOptions);
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

        public async Task<bool> UpdatePageAsync(Guid id, string title, int displayWidth, int displayHeight, string orientation, string thumbnailUrl)
        {
            try
            {
                // First update the basic page info
                var updateRequest = new
                {
                    Id = id,
                    Title = title,
                    Elements = new List<object>() // Keep existing elements
                };

                var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/page/{id}", content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to update page {PageId}. Status: {StatusCode}", id, response.StatusCode);
                    return false;
                }

                // Update display size if needed
                var displaySizeRequest = new
                {
                    Width = displayWidth,
                    Height = displayHeight,
                    Orientation = orientation
                };

                var displayJson = JsonSerializer.Serialize(displaySizeRequest, _jsonOptions);
                var displayContent = new StringContent(displayJson, Encoding.UTF8, "application/json");

                var displayResponse = await _httpClient.PutAsync($"api/page/{id}/display-size", displayContent);
                if (!displayResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to update page {PageId} display size. Status: {StatusCode}", id, displayResponse.StatusCode);
                    return false;
                }

                // Update thumbnail if provided
                if (!string.IsNullOrEmpty(thumbnailUrl))
                {
                    var thumbnailRequest = new { ThumbnailUrl = thumbnailUrl };
                    var thumbnailJson = JsonSerializer.Serialize(thumbnailRequest, _jsonOptions);
                    var thumbnailContent = new StringContent(thumbnailJson, Encoding.UTF8, "application/json");

                    var thumbnailResponse = await _httpClient.PutAsync($"api/page/{id}/thumbnail", thumbnailContent);
                    if (!thumbnailResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Failed to update page {PageId} thumbnail. Status: {StatusCode}", id, thumbnailResponse.StatusCode);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {PageId} via API Gateway", id);
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

        public async Task<bool> TogglePageStatusAsync(Guid id, string status)
        {
            try
            {
                var request = new { Status = status };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/page/{id}/status", content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                _logger.LogWarning("Failed to toggle page {PageId} status via API Gateway. Status: {StatusCode}", id, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling page {PageId} status via API Gateway", id);
                return false;
            }
        }

        public async Task<bool> SetPageThumbnailAsync(Guid id, string thumbnailUrl)
        {
            try
            {
                var request = new { ThumbnailUrl = thumbnailUrl };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/page/{id}/thumbnail", content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                _logger.LogWarning("Failed to set page {PageId} thumbnail via API Gateway. Status: {StatusCode}", id, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting page {PageId} thumbnail via API Gateway", id);
                return false;
            }
        }
    }
}

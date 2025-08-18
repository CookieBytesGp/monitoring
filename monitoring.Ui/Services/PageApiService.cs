using Monitoring.Ui.Models.Page;
using Monitoring.Ui.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Monitoring.Ui.Services
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public int? Count { get; set; }
    }

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
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() }
            };

            // Base address should point to API Gateway
            var baseUrl = configuration["ApiGateway:BaseUrl"] ?? "http://localhost:7001";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<List<PageViewModel>> GetAllPagesAsync()
        {
            try
            {
                _logger.LogInformation("Requesting pages from API Gateway at {BaseUrl}", _httpClient.BaseAddress);
                
                var response = await _httpClient.GetAsync("api/page");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Received API response: {Json}", json);
                    
                    // Parse the JSON as JsonDocument to handle dynamic structure
                    using var document = JsonDocument.Parse(json);
                    var root = document.RootElement;
                    
                    // Check if it's a structured response with success/data format
                    if (root.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
                    {
                        if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                        {
                            var pages = JsonSerializer.Deserialize<List<PageViewModel>>(dataProp.GetRawText(), _jsonOptions);
                            _logger.LogInformation("Successfully parsed structured response with {Count} pages", pages?.Count ?? 0);
                            return pages ?? new List<PageViewModel>();
                        }
                        else
                        {
                            _logger.LogWarning("Structured response has no data array");
                            return new List<PageViewModel>();
                        }
                    }
                    else
                    {
                        // Try to parse as direct array
                        var pages = JsonSerializer.Deserialize<List<PageViewModel>>(json, _jsonOptions);
                        _logger.LogInformation("Successfully parsed direct array with {Count} pages", pages?.Count ?? 0);
                        return pages ?? new List<PageViewModel>();
                    }
                }
                
                _logger.LogWarning("Failed to get pages from API Gateway. Status: {StatusCode}, Reason: {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                    
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error response content: {ErrorContent}", errorContent);
                
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
                _logger.LogInformation("Requesting page {PageId} from API Gateway", id);
                
                var response = await _httpClient.GetAsync($"api/page/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Received page response: {ResponseLength} characters", json.Length);
                    
                    // Try to parse as new structured response format first
                    try
                    {
                        var structuredResponse = JsonSerializer.Deserialize<ApiResponse<PageViewModel>>(json, _jsonOptions);
                        if (structuredResponse?.Success == true && structuredResponse.Data != null)
                        {
                            _logger.LogInformation("Successfully parsed structured response for page {PageId}", id);
                            return structuredResponse.Data;
                        }
                    }
                    catch (JsonException)
                    {
                        _logger.LogInformation("Failed to parse as structured response, trying direct object parse");
                    }
                    
                    // Fallback to direct object parsing
                    var page = JsonSerializer.Deserialize<PageViewModel>(json, _jsonOptions);
                    _logger.LogInformation("Successfully parsed direct object for page {PageId}", id);
                    return page;
                }
                
                _logger.LogWarning("Failed to get page {PageId} from API Gateway. Status: {StatusCode}, Reason: {ReasonPhrase}", 
                    id, response.StatusCode, response.ReasonPhrase);
                    
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error response content: {ErrorContent}", errorContent);
                
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
                _logger.LogInformation("Creating page with title: {Title}", request.Title);
                
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/page", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Create page response: {ResponseLength} characters", responseJson.Length);
                    
                    // Try to parse as new structured response format first
                    try
                    {
                        var structuredResponse = JsonSerializer.Deserialize<ApiResponse<PageViewModel>>(responseJson, _jsonOptions);
                        if (structuredResponse?.Success == true && structuredResponse.Data != null)
                        {
                            _logger.LogInformation("Successfully created page with structured response");
                            return structuredResponse.Data;
                        }
                    }
                    catch (JsonException)
                    {
                        _logger.LogInformation("Failed to parse as structured response, trying direct object parse");
                    }
                    
                    // Fallback to direct object parsing
                    var page = JsonSerializer.Deserialize<PageViewModel>(responseJson, _jsonOptions);
                    _logger.LogInformation("Successfully created page with direct response parse");
                    return page;
                }

                _logger.LogWarning("Failed to create page. Status: {StatusCode}, Reason: {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                    
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Create page error content: {ErrorContent}", errorContent);
                
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

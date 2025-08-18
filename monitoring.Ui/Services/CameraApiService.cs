using Monitoring.Ui.Models.Camera;
using Newtonsoft.Json;
using System.Text;

namespace Monitoring.Ui.Services;

public interface ICameraApiService
{
    Task<List<CameraViewModel>> GetAllCamerasAsync();
    Task<CameraViewModel?> GetCameraByIdAsync(Guid id);
    Task<CameraViewModel?> CreateCameraAsync(CreateCameraViewModel model);
    Task<bool> TestConnectionAsync(Guid id);
    Task<CameraConnectionViewModel?> ConnectAsync(Guid id);
    Task<List<string>> GetSupportedStrategiesAsync(Guid id);
    Task<Dictionary<string, bool>> TestAllStrategiesAsync(Guid id);
    Task<byte[]?> GetSnapshotAsync(Guid id);
    Task<string?> GetStreamUrlAsync(Guid id);
}

public class CameraApiService : ICameraApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CameraApiService> _logger;
    private readonly string _baseUrl;

    public CameraApiService(HttpClient httpClient, IConfiguration configuration, ILogger<CameraApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["ApiGateway:BaseUrl"] ?? "http://localhost:7001";
    }

    public async Task<List<CameraViewModel>> GetAllCamerasAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/camera");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CameraViewModel>>(json) ?? new List<CameraViewModel>();
            }
            
            _logger.LogError("Failed to get cameras. Status: {StatusCode}", response.StatusCode);
            return new List<CameraViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting cameras from API");
            return new List<CameraViewModel>();
        }
    }

    public async Task<CameraViewModel?> GetCameraByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/camera/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CameraViewModel>(json);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting camera {CameraId} from API", id);
            return null;
        }
    }

    public async Task<CameraViewModel?> CreateCameraAsync(CreateCameraViewModel model)
    {
        try
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/camera", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CameraViewModel>(responseJson);
            }
            
            // Log response content for debugging
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("API returned error. Status: {StatusCode}, Content: {Content}", 
                response.StatusCode, errorContent);
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating camera via API");
            return null;
        }
    }

    public async Task<bool> TestConnectionAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/camera/{id}/test-connection", null);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(json);
                return result?.isConnected ?? false;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while testing connection for camera {CameraId}", id);
            return false;
        }
    }

    public async Task<CameraConnectionViewModel?> ConnectAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/camera/{id}/connect", null);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CameraConnectionViewModel>(json);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while connecting to camera {CameraId}", id);
            return null;
        }
    }

    public async Task<List<string>> GetSupportedStrategiesAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/camera/{id}/supported-strategies");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            
            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting supported strategies for camera {CameraId}", id);
            return new List<string>();
        }
    }

    public async Task<Dictionary<string, bool>> TestAllStrategiesAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/camera/{id}/test-all-strategies", null);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, bool>>(json) ?? new Dictionary<string, bool>();
            }
            
            return new Dictionary<string, bool>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while testing all strategies for camera {CameraId}", id);
            return new Dictionary<string, bool>();
        }
    }

    public async Task<byte[]?> GetSnapshotAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/camera/{id}/snapshot");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting snapshot for camera {CameraId}", id);
            return null;
        }
    }

    public async Task<string?> GetStreamUrlAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/camera/{id}/stream");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(json);
                return result?.streamUrl;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting stream URL for camera {CameraId}", id);
            return null;
        }
    }
}

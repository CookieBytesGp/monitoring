using FluentResults;
using Microsoft.Extensions.Logging;
using Monitoring.Domain.Aggregates.Camera;
using Monitoring.Domain.Aggregates.Camera.ValueObjects;
using Monitoring.Domain.Services.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Monitoring.Infrastructure.Camera.Strategies
{
    /// <summary>
    /// استراتژی اتصال ONVIF - برای دوربین‌های استاندارد ONVIF
    /// </summary>
    public class ONVIFCameraStrategy : BaseCameraStrategy, ICameraConnectionStrategy
    {
        private readonly Dictionary<string, ONVIFDevice> _discoveredDevices;
        private const int ONVIF_DISCOVERY_PORT = 3702;
        private const int ONVIF_DEVICE_PORT = 80;

        public ONVIFCameraStrategy(ILogger<ONVIFCameraStrategy> logger, HttpClient httpClient) 
            : base(logger, httpClient)
        {
            _discoveredDevices = new Dictionary<string, ONVIFDevice>();
        }

        #region Properties

        public string StrategyName => "ONVIF";
        public int Priority => 15; // اولویت بالا برای استاندارد ONVIF

        #endregion

        #region Public Methods

        public bool SupportsCamera(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            if (camera?.Type == null) return false;

            var typeName = camera.Type.Name.ToLower();
            var hasOnvifInType = typeName.Contains("onvif") || typeName.Contains("ip") || typeName.Contains("network");

            // بررسی پورت ONVIF در آدرس
            var hasOnvifPort = camera.Location?.Value?.Contains(":3702") == true ||
                              camera.Location?.Value?.Contains("onvif") == true;

            // بررسی تنظیمات ONVIF در Configuration
            var hasOnvifConfig = camera.Configuration?.AdditionalSettings?.Any(x => 
                x.Key.ToLower().Contains("onvif") || (x.Value?.ToLower().Contains("onvif") == true)) == true;

            return hasOnvifInType || hasOnvifPort || hasOnvifConfig;
        }

        public async Task<Result<bool>> TestConnectionAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Testing ONVIF connection for camera {CameraName}", camera.Name);

                if (!SupportsCamera(camera))
                    return Result.Fail<bool>("Camera is not supported by ONVIF strategy");

                // ONVIF Device Discovery
                var discoveryResult = await DiscoverONVIFDeviceAsync(camera);
                if (discoveryResult.IsFailed)
                    return Result.Fail<bool>(discoveryResult.Errors);

                var device = discoveryResult.Value;

                // تست GetDeviceInformation
                var deviceInfoResult = await GetDeviceInformationAsync(device);
                if (deviceInfoResult.IsFailed)
                    return Result.Fail<bool>("Failed to get ONVIF device information");

                // تست GetCapabilities
                var capabilitiesResult = await GetCapabilitiesAsync(device);
                if (capabilitiesResult.IsFailed)
                    return Result.Fail<bool>("Failed to get ONVIF device capabilities");

                LogSuccess("TestConnection", camera);
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                LogError(ex, "TestConnection", camera);
                return Result.Fail<bool>($"ONVIF connection test failed: {ex.Message}");
            }
        }

        public async Task<Result<string>> GetStreamUrlAsync(Monitoring.Domain.Aggregates.Camera.Camera camera, string quality = "high")
        {
            try
            {
                if (!SupportsCamera(camera))
                    return Result.Fail<string>("Camera is not supported by ONVIF strategy");

                var discoveryResult = await DiscoverONVIFDeviceAsync(camera);
                if (discoveryResult.IsFailed)
                    return Result.Fail<string>(discoveryResult.Errors);

                var device = discoveryResult.Value;

                // دریافت Media Service URL
                var mediaServiceResult = await GetMediaServiceUrlAsync(device);
                if (mediaServiceResult.IsFailed)
                    return Result.Fail<string>(mediaServiceResult.Errors);

                // دریافت Stream URL از Media Service
                var streamUrlResult = await GetStreamUriAsync(device, mediaServiceResult.Value, quality);
                if (streamUrlResult.IsFailed)
                    return Result.Fail<string>(streamUrlResult.Errors);

                _logger.LogDebug("Generated ONVIF stream URL for camera {CameraName}: {StreamUrl}", 
                    camera.Name, streamUrlResult.Value);

                return Result.Ok(streamUrlResult.Value);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetStreamUrl", camera, new { Quality = quality });
                return Result.Fail<string>($"Failed to generate ONVIF stream URL: {ex.Message}");
            }
        }

        public async Task<Result<byte[]>> CaptureSnapshotAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Capturing ONVIF snapshot from camera {CameraName}", camera.Name);

                var discoveryResult = await DiscoverONVIFDeviceAsync(camera);
                if (discoveryResult.IsFailed)
                    return Result.Fail<byte[]>(discoveryResult.Errors);

                var device = discoveryResult.Value;

                // دریافت Snapshot URL
                var snapshotUrlResult = await GetSnapshotUriAsync(device);
                if (snapshotUrlResult.IsFailed)
                    return Result.Fail<byte[]>(snapshotUrlResult.Errors);

                // دانلود تصویر
                var imageBytes = await DownloadImageFromUrlAsync(snapshotUrlResult.Value, device);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    LogSuccess("CaptureSnapshot", camera, new { Size = imageBytes.Length });
                    return Result.Ok(imageBytes);
                }
                else
                {
                    return Result.Fail<byte[]>("Failed to capture snapshot from ONVIF device");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "CaptureSnapshot", camera);
                return Result.Fail<byte[]>($"ONVIF snapshot capture failed: {ex.Message}");
            }
        }

        public async Task<Result<CameraConnectionInfo>> ConnectAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Establishing ONVIF connection for camera {CameraName}", camera.Name);

                var testResult = await TestConnectionAsync(camera);
                if (testResult.IsFailed)
                    return Result.Fail<CameraConnectionInfo>(testResult.Errors);

                var discoveryResult = await DiscoverONVIFDeviceAsync(camera);
                if (discoveryResult.IsFailed)
                    return Result.Fail<CameraConnectionInfo>(discoveryResult.Errors);

                var device = discoveryResult.Value;

                // دریافت URLs
                var streamUrlResult = await GetStreamUrlAsync(camera);
                var snapshotUrlResult = await GetSnapshotUriAsync(device);

                var streamUrl = streamUrlResult.IsSuccess ? streamUrlResult.Value : "";
                var snapshotUrl = snapshotUrlResult.IsSuccess ? snapshotUrlResult.Value : streamUrl;

                var connectionInfoResult = CameraConnectionInfo.Create(
                    streamUrl: streamUrl,
                    snapshotUrl: snapshotUrl,
                    isConnected: true,
                    connectionType: StrategyName
                );

                if (connectionInfoResult.IsFailed)
                    return Result.Fail<CameraConnectionInfo>(connectionInfoResult.Errors);

                var connectionInfo = connectionInfoResult.Value
                    .AddInfo("protocol", "ONVIF")
                    .AddInfo("device_service_url", device.ServiceUrl)
                    .AddInfo("manufacturer", device.Manufacturer ?? "Unknown")
                    .AddInfo("model", device.Model ?? "Unknown")
                    .AddInfo("firmware_version", device.FirmwareVersion ?? "Unknown")
                    .AddInfo("onvif_version", device.ONVIFVersion ?? "Unknown");

                LogSuccess("Connect", camera);
                return Result.Ok(connectionInfo);
            }
            catch (Exception ex)
            {
                LogError(ex, "Connect", camera);
                return Result.Fail<CameraConnectionInfo>($"ONVIF connection failed: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DisconnectAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                _logger.LogInformation("Disconnecting ONVIF connection for camera {CameraName}", camera.Name);

                // در ONVIF معمولاً نیاز به disconnect خاصی نیست
                // فقط cleanup local cache
                var cacheKey = GenerateCacheKey(camera);
                _discoveredDevices.Remove(cacheKey);

                await Task.CompletedTask;
                LogSuccess("Disconnect", camera);
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                LogError(ex, "Disconnect", camera);
                return Result.Fail<bool>($"ONVIF disconnection failed: {ex.Message}");
            }
        }

        public async Task<Result<List<string>>> GetCapabilitiesAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var discoveryResult = await DiscoverONVIFDeviceAsync(camera);
                if (discoveryResult.IsFailed)
                    return Result.Fail<List<string>>(discoveryResult.Errors);

                var device = discoveryResult.Value;
                var capabilitiesResult = await GetCapabilitiesAsync(device);
                
                if (capabilitiesResult.IsFailed)
                    return Result.Fail<List<string>>(capabilitiesResult.Errors);

                return Result.Ok(capabilitiesResult.Value);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetCapabilities", camera);
                return Result.Fail<List<string>>($"Failed to get ONVIF capabilities: {ex.Message}");
            }
        }

        public async Task<Result<bool>> SetStreamQualityAsync(Monitoring.Domain.Aggregates.Camera.Camera camera, string quality)
        {
            try
            {
                _logger.LogInformation("Setting ONVIF stream quality to {Quality} for camera {CameraName}", 
                    quality, camera.Name);

                // ONVIF quality setting would require specific profile configuration
                // For now, we'll just validate and return success
                var validQualities = new[] { "high", "medium", "low" };
                if (!validQualities.Contains(quality.ToLower()))
                    return Result.Fail<bool>($"Invalid quality setting: {quality}");

                await Task.CompletedTask;
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                LogError(ex, "SetStreamQuality", camera, new { Quality = quality });
                return Result.Fail<bool>($"Failed to set ONVIF stream quality: {ex.Message}");
            }
        }

        public async Task<Result<Dictionary<string, object>>> GetCameraStatusAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var discoveryResult = await DiscoverONVIFDeviceAsync(camera);
                var isConnected = discoveryResult.IsSuccess;

                ONVIFDevice device = null;
                if (isConnected)
                    device = discoveryResult.Value;

                var status = new Dictionary<string, object>
                {
                    ["strategy"] = StrategyName,
                    ["protocol"] = "ONVIF",
                    ["is_connected"] = isConnected,
                    ["device_service_url"] = device?.ServiceUrl ?? "unknown",
                    ["manufacturer"] = device?.Manufacturer ?? "unknown",
                    ["model"] = device?.Model ?? "unknown",
                    ["firmware_version"] = device?.FirmwareVersion ?? "unknown",
                    ["onvif_version"] = device?.ONVIFVersion ?? "unknown",
                    ["supported_qualities"] = new[] { "high", "medium", "low" },
                    ["last_check"] = DateTime.UtcNow
                };

                return Result.Ok(status);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetCameraStatus", camera);
                return Result.Fail<Dictionary<string, object>>($"Failed to get ONVIF camera status: {ex.Message}");
            }
        }

        #endregion

        #region Protected Override Methods

        protected override async Task<Result<bool>> PerformHealthCheckAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var discoveryResult = await DiscoverONVIFDeviceAsync(camera);
                if (discoveryResult.IsFailed)
                    return Result.Fail<bool>("ONVIF device discovery failed");

                var device = discoveryResult.Value;
                var deviceInfoResult = await GetDeviceInformationAsync(device);
                
                return deviceInfoResult.IsSuccess ? 
                    Result.Ok(true) : 
                    Result.Fail<bool>("ONVIF device health check failed");
            }
            catch (Exception ex)
            {
                LogError(ex, "PerformHealthCheck", camera);
                return Result.Fail<bool>($"ONVIF health check failed: {ex.Message}");
            }
        }

        #endregion

        #region Private ONVIF Methods

        private async Task<Result<ONVIFDevice>> DiscoverONVIFDeviceAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var cacheKey = GenerateCacheKey(camera);
                
                // Check cache first
                if (_discoveredDevices.TryGetValue(cacheKey, out var cachedDevice))
                {
                    return Result.Ok(cachedDevice);
                }

                var deviceUrl = BuildDeviceServiceUrl(camera);
                var device = new ONVIFDevice
                {
                    ServiceUrl = deviceUrl,
                    Address = ExtractAddressFromCamera(camera)
                };

                // Get device information
                var deviceInfoResult = await GetDeviceInformationAsync(device);
                if (deviceInfoResult.IsSuccess)
                {
                    var deviceInfo = deviceInfoResult.Value;
                    device.Manufacturer = deviceInfo.ContainsKey("Manufacturer") ? deviceInfo["Manufacturer"].ToString() : null;
                    device.Model = deviceInfo.ContainsKey("Model") ? deviceInfo["Model"].ToString() : null;
                    device.FirmwareVersion = deviceInfo.ContainsKey("FirmwareVersion") ? deviceInfo["FirmwareVersion"].ToString() : null;
                }

                // Cache the device
                _discoveredDevices[cacheKey] = device;

                return Result.Ok(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering ONVIF device for camera {CameraName}", camera.Name);
                return Result.Fail<ONVIFDevice>($"ONVIF device discovery failed: {ex.Message}");
            }
        }

        private string BuildDeviceServiceUrl(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            var address = ExtractAddressFromCamera(camera);
            
            // Remove protocol if present
            if (address.StartsWith("http://"))
                address = address.Substring(7);
            if (address.StartsWith("https://"))
                address = address.Substring(8);

            // Add port if not present
            if (!address.Contains(":"))
                address += $":{ONVIF_DEVICE_PORT}";

            return $"http://{address}/onvif/device_service";
        }

        private string ExtractAddressFromCamera(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            if (!string.IsNullOrEmpty(camera.Location?.Value))
                return camera.Location.Value;

            if (!string.IsNullOrEmpty(camera.ConnectionInfo?.StreamUrl))
            {
                var uri = new Uri(camera.ConnectionInfo.StreamUrl);
                return $"{uri.Host}:{uri.Port}";
            }

            throw new InvalidOperationException("Cannot extract address from camera");
        }

        private string GenerateCacheKey(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            return $"{camera.Name}_{ExtractAddressFromCamera(camera)}";
        }

        private async Task<Result<Dictionary<string, object>>> GetDeviceInformationAsync(ONVIFDevice device)
        {
            try
            {
                var soapRequest = BuildGetDeviceInformationRequest();
                var response = await SendSOAPRequestAsync(device.ServiceUrl, soapRequest);
                
                if (response.IsFailed)
                    return Result.Fail<Dictionary<string, object>>(response.Errors);

                var deviceInfo = ParseDeviceInformationResponse(response.Value);
                return Result.Ok(deviceInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device information from {ServiceUrl}", device.ServiceUrl);
                return Result.Fail<Dictionary<string, object>>($"Get device information failed: {ex.Message}");
            }
        }

        private async Task<Result<List<string>>> GetCapabilitiesAsync(ONVIFDevice device)
        {
            try
            {
                var soapRequest = BuildGetCapabilitiesRequest();
                var response = await SendSOAPRequestAsync(device.ServiceUrl, soapRequest);
                
                if (response.IsFailed)
                    return Result.Fail<List<string>>(response.Errors);

                var capabilities = ParseCapabilitiesResponse(response.Value);
                return Result.Ok(capabilities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting capabilities from {ServiceUrl}", device.ServiceUrl);
                return Result.Fail<List<string>>($"Get capabilities failed: {ex.Message}");
            }
        }

        private async Task<Result<string>> GetMediaServiceUrlAsync(ONVIFDevice device)
        {
            try
            {
                var capabilitiesResult = await GetCapabilitiesAsync(device);
                if (capabilitiesResult.IsFailed)
                    return Result.Fail<string>("Failed to get device capabilities");

                // Extract media service URL from capabilities
                // This is a simplified implementation
                var baseUrl = device.ServiceUrl.Replace("/onvif/device_service", "");
                var mediaServiceUrl = $"{baseUrl}/onvif/media_service";

                return Result.Ok(mediaServiceUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting media service URL for device {ServiceUrl}", device.ServiceUrl);
                return Result.Fail<string>($"Get media service URL failed: {ex.Message}");
            }
        }

        private async Task<Result<string>> GetStreamUriAsync(ONVIFDevice device, string mediaServiceUrl, string quality)
        {
            try
            {
                var soapRequest = BuildGetStreamUriRequest(quality);
                var response = await SendSOAPRequestAsync(mediaServiceUrl, soapRequest);
                
                if (response.IsFailed)
                    return Result.Fail<string>(response.Errors);

                var streamUri = ParseStreamUriResponse(response.Value);
                return Result.Ok(streamUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream URI from {MediaServiceUrl}", mediaServiceUrl);
                return Result.Fail<string>($"Get stream URI failed: {ex.Message}");
            }
        }

        private async Task<Result<string>> GetSnapshotUriAsync(ONVIFDevice device)
        {
            try
            {
                var mediaServiceResult = await GetMediaServiceUrlAsync(device);
                if (mediaServiceResult.IsFailed)
                    return Result.Fail<string>(mediaServiceResult.Errors);

                var soapRequest = BuildGetSnapshotUriRequest();
                var response = await SendSOAPRequestAsync(mediaServiceResult.Value, soapRequest);
                
                if (response.IsFailed)
                    return Result.Fail<string>(response.Errors);

                var snapshotUri = ParseSnapshotUriResponse(response.Value);
                return Result.Ok(snapshotUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting snapshot URI for device {ServiceUrl}", device.ServiceUrl);
                return Result.Fail<string>($"Get snapshot URI failed: {ex.Message}");
            }
        }

        private async Task<Result<string>> SendSOAPRequestAsync(string serviceUrl, string soapRequest)
        {
            try
            {
                var content = new StringContent(soapRequest, Encoding.UTF8, "application/soap+xml");
                content.Headers.Add("SOAPAction", "\"\"");

                var response = await _httpClient.PostAsync(serviceUrl, content);
                
                if (!response.IsSuccessStatusCode)
                    return Result.Fail<string>($"SOAP request failed with status {response.StatusCode}");

                var responseContent = await response.Content.ReadAsStringAsync();
                return Result.Ok(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SOAP request to {ServiceUrl}", serviceUrl);
                return Result.Fail<string>($"SOAP request failed: {ex.Message}");
            }
        }

        #endregion

        #region SOAP Request Builders

        private string BuildGetDeviceInformationRequest()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:tds=""http://www.onvif.org/ver10/device/wsdl"">
    <soap:Header/>
    <soap:Body>
        <tds:GetDeviceInformation/>
    </soap:Body>
</soap:Envelope>";
        }

        private string BuildGetCapabilitiesRequest()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:tds=""http://www.onvif.org/ver10/device/wsdl"">
    <soap:Header/>
    <soap:Body>
        <tds:GetCapabilities>
            <tds:Category>All</tds:Category>
        </tds:GetCapabilities>
    </soap:Body>
</soap:Envelope>";
        }

        private string BuildGetStreamUriRequest(string quality)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:trt=""http://www.onvif.org/ver10/media/wsdl"" xmlns:tt=""http://www.onvif.org/ver10/schema"">
    <soap:Header/>
    <soap:Body>
        <trt:GetStreamUri>
            <trt:StreamSetup>
                <tt:Stream>RTP-Unicast</tt:Stream>
                <tt:Transport>
                    <tt:Protocol>RTSP</tt:Protocol>
                </tt:Transport>
            </trt:StreamSetup>
            <trt:ProfileToken>Profile_1</trt:ProfileToken>
        </trt:GetStreamUri>
    </soap:Body>
</soap:Envelope>";
        }

        private string BuildGetSnapshotUriRequest()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:trt=""http://www.onvif.org/ver10/media/wsdl"">
    <soap:Header/>
    <soap:Body>
        <trt:GetSnapshotUri>
            <trt:ProfileToken>Profile_1</trt:ProfileToken>
        </trt:GetSnapshotUri>
    </soap:Body>
</soap:Envelope>";
        }

        #endregion

        #region Response Parsers

        private Dictionary<string, object> ParseDeviceInformationResponse(string response)
        {
            try
            {
                var doc = XDocument.Parse(response);
                var ns = XNamespace.Get("http://www.onvif.org/ver10/device/wsdl");

                var deviceInfo = new Dictionary<string, object>();

                var responseElement = doc.Descendants(ns + "GetDeviceInformationResponse").FirstOrDefault();
                if (responseElement != null)
                {
                    deviceInfo["Manufacturer"] = responseElement.Element(ns + "Manufacturer")?.Value ?? "Unknown";
                    deviceInfo["Model"] = responseElement.Element(ns + "Model")?.Value ?? "Unknown";
                    deviceInfo["FirmwareVersion"] = responseElement.Element(ns + "FirmwareVersion")?.Value ?? "Unknown";
                    deviceInfo["SerialNumber"] = responseElement.Element(ns + "SerialNumber")?.Value ?? "Unknown";
                    deviceInfo["HardwareId"] = responseElement.Element(ns + "HardwareId")?.Value ?? "Unknown";
                }

                return deviceInfo;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing device information response");
                return new Dictionary<string, object>();
            }
        }

        private List<string> ParseCapabilitiesResponse(string response)
        {
            try
            {
                var capabilities = new List<string>
                {
                    "onvif_standard",
                    "device_management",
                    "media_service"
                };

                // Parse actual capabilities from response
                var doc = XDocument.Parse(response);
                
                if (doc.ToString().Contains("Media"))
                    capabilities.Add("media_streaming");
                if (doc.ToString().Contains("PTZ"))
                    capabilities.Add("ptz_control");
                if (doc.ToString().Contains("Imaging"))
                    capabilities.Add("imaging_control");
                if (doc.ToString().Contains("Events"))
                    capabilities.Add("event_handling");

                return capabilities;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing capabilities response");
                return new List<string> { "onvif_standard" };
            }
        }

        private string ParseStreamUriResponse(string response)
        {
            try
            {
                var doc = XDocument.Parse(response);
                var ns = XNamespace.Get("http://www.onvif.org/ver10/media/wsdl");

                var uriElement = doc.Descendants(ns + "Uri").FirstOrDefault();
                return uriElement?.Value ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing stream URI response");
                return "";
            }
        }

        private string ParseSnapshotUriResponse(string response)
        {
            try
            {
                var doc = XDocument.Parse(response);
                var ns = XNamespace.Get("http://www.onvif.org/ver10/media/wsdl");

                var uriElement = doc.Descendants(ns + "Uri").FirstOrDefault();
                return uriElement?.Value ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing snapshot URI response");
                return "";
            }
        }

        private async Task<byte[]> DownloadImageFromUrlAsync(string imageUrl, ONVIFDevice device)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
                
                // اضافه کردن authentication اگر نیاز باشد
                if (!string.IsNullOrEmpty(device.Username) && !string.IsNullOrEmpty(device.Password))
                {
                    var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{device.Username}:{device.Password}"));
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
                }

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                _logger.LogWarning("Failed to download image from ONVIF device. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading image from ONVIF device: {Url}", imageUrl);
                return null;
            }
        }

        #endregion
    }

    /// <summary>
    /// مدل ONVIF Device
    /// </summary>
    public class ONVIFDevice
    {
        public string ServiceUrl { get; set; }
        public string Address { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string FirmwareVersion { get; set; }
        public string SerialNumber { get; set; }
        public string HardwareId { get; set; }
        public string ONVIFVersion { get; set; } = "2.0";
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Monitoring.Ui.Controllers
{
    public class UploadController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UploadController> _logger;
        private readonly IConfiguration _configuration;

        public UploadController(IHttpClientFactory httpClientFactory, ILogger<UploadController> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// آپلود تصویر پس‌زمینه
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BackgroundImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "فایل انتخاب نشده است" });
                }

                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiGateway:BaseUrl"] ?? "http://localhost:7001";

                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.FileName);

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/upload/background-image", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<dynamic>(responseContent);
                    return Json(result);
                }
                else
                {
                    var error = JsonSerializer.Deserialize<dynamic>(responseContent);
                    return Json(error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading background image");
                return Json(new { success = false, message = "خطای داخلی سرور" });
            }
        }

        /// <summary>
        /// آپلود صوت پس‌زمینه
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BackgroundAudio(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "فایل انتخاب نشده است" });
                }

                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiGateway:BaseUrl"] ?? "http://localhost:7001";

                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.FileName);

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/upload/background-audio", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<dynamic>(responseContent);
                    return Json(result);
                }
                else
                {
                    var error = JsonSerializer.Deserialize<dynamic>(responseContent);
                    return Json(error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading background audio");
                return Json(new { success = false, message = "خطای داخلی سرور" });
            }
        }

        /// <summary>
        /// آپلود ویدیو پس‌زمینه
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BackgroundVideo(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "فایل انتخاب نشده است" });
                }

                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiGateway:BaseUrl"] ?? "http://localhost:7001";

                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.FileName);

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/upload/background-video", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<dynamic>(responseContent);
                    return Json(result);
                }
                else
                {
                    var error = JsonSerializer.Deserialize<dynamic>(responseContent);
                    return Json(error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading background video");
                return Json(new { success = false, message = "خطای داخلی سرور" });
            }
        }
    }
}

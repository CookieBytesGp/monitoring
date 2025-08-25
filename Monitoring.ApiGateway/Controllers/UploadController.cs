using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Monitoring.ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UploadController> _logger;
    private readonly IConfiguration _configuration;

    public UploadController(IWebHostEnvironment environment, ILogger<UploadController> logger, IConfiguration configuration)
    {
        _environment = environment;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// آپلود تصویر پس‌زمینه
    /// </summary>
    [HttpPost("background-image")]
    public async Task<IActionResult> UploadBackgroundImage([Required] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "فایل انتخاب نشده است" 
                });
            }

            // بررسی فرمت فایل
            var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedImageTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { 
                    success = false, 
                    message = "فرمت فایل پشتیبانی نمی‌شود. فرمت‌های مجاز: JPG, PNG, GIF, WebP" 
                });
            }

            // بررسی اندازه فایل (حداکثر 10MB)
            const long maxSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxSize)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "اندازه فایل نباید بیشتر از 10 مگابایت باشد" 
                });
            }

            // ایجاد پوشه در صورت عدم وجود
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "backgrounds");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // تولید نام یکتا برای فایل
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // ذخیره فایل
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // تولید URL
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var fileUrl = $"{baseUrl}/uploads/backgrounds/{fileName}";

            _logger.LogInformation("Background image uploaded successfully: {FileName}", fileName);

            return Ok(new { 
                success = true, 
                message = "تصویر با موفقیت آپلود شد",
                data = new {
                    url = fileUrl,
                    fileName = fileName,
                    originalName = file.FileName,
                    size = file.Length,
                    type = "image"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading background image");
            return StatusCode(500, new { 
                success = false, 
                message = "خطای داخلی سرور", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// آپلود صوت پس‌زمینه
    /// </summary>
    [HttpPost("background-audio")]
    public async Task<IActionResult> UploadBackgroundAudio([Required] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "فایل انتخاب نشده است" 
                });
            }

            // بررسی فرمت فایل
            var allowedAudioTypes = new[] { "audio/mpeg", "audio/mp3", "audio/wav", "audio/ogg", "audio/m4a" };
            if (!allowedAudioTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { 
                    success = false, 
                    message = "فرمت فایل پشتیبانی نمی‌شود. فرمت‌های مجاز: MP3, WAV, OGG, M4A" 
                });
            }

            // بررسی اندازه فایل (حداکثر 50MB)
            const long maxSize = 50 * 1024 * 1024; // 50MB
            if (file.Length > maxSize)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "اندازه فایل نباید بیشتر از 50 مگابایت باشد" 
                });
            }

            // ایجاد پوشه در صورت عدم وجود
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "audio");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // تولید نام یکتا برای فایل
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // ذخیره فایل
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // تولید URL
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var fileUrl = $"{baseUrl}/uploads/audio/{fileName}";

            _logger.LogInformation("Background audio uploaded successfully: {FileName}", fileName);

            return Ok(new { 
                success = true, 
                message = "فایل صوتی با موفقیت آپلود شد",
                data = new {
                    url = fileUrl,
                    fileName = fileName,
                    originalName = file.FileName,
                    size = file.Length,
                    type = "audio"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading background audio");
            return StatusCode(500, new { 
                success = false, 
                message = "خطای داخلی سرور", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// آپلود ویدیو پس‌زمینه
    /// </summary>
    [HttpPost("background-video")]
    public async Task<IActionResult> UploadBackgroundVideo([Required] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "فایل انتخاب نشده است" 
                });
            }

            // بررسی فرمت فایل
            var allowedVideoTypes = new[] { "video/mp4", "video/webm", "video/ogg", "video/avi", "video/mov" };
            if (!allowedVideoTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { 
                    success = false, 
                    message = "فرمت فایل پشتیبانی نمی‌شود. فرمت‌های مجاز: MP4, WebM, OGG, AVI, MOV" 
                });
            }

            // بررسی اندازه فایل (حداکثر 100MB)
            const long maxSize = 100 * 1024 * 1024; // 100MB
            if (file.Length > maxSize)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "اندازه فایل نباید بیشتر از 100 مگابایت باشد" 
                });
            }

            // ایجاد پوشه در صورت عدم وجود
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "videos");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // تولید نام یکتا برای فایل
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // ذخیره فایل
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // تولید URL
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var fileUrl = $"{baseUrl}/uploads/videos/{fileName}";

            _logger.LogInformation("Background video uploaded successfully: {FileName}", fileName);

            return Ok(new { 
                success = true, 
                message = "ویدیو با موفقیت آپلود شد",
                data = new {
                    url = fileUrl,
                    fileName = fileName,
                    originalName = file.FileName,
                    size = file.Length,
                    type = "video"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading background video");
            return StatusCode(500, new { 
                success = false, 
                message = "خطای داخلی سرور", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// حذف فایل آپلود شده
    /// </summary>
    [HttpDelete]
    public IActionResult DeleteFile([FromQuery] string fileName, [FromQuery] string type)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(type))
            {
                return BadRequest(new { 
                    success = false, 
                    message = "نام فایل و نوع آن الزامی است" 
                });
            }

            var validTypes = new[] { "backgrounds", "audio", "videos" };
            if (!validTypes.Contains(type.ToLower()))
            {
                return BadRequest(new { 
                    success = false, 
                    message = "نوع فایل نامعتبر است" 
                });
            }

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", type.ToLower(), fileName);
            
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { 
                    success = false, 
                    message = "فایل یافت نشد" 
                });
            }

            System.IO.File.Delete(filePath);

            _logger.LogInformation("File deleted successfully: {FileName}", fileName);

            return Ok(new { 
                success = true, 
                message = "فایل با موفقیت حذف شد" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
            return StatusCode(500, new { 
                success = false, 
                message = "خطای داخلی سرور", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// دریافت لیست فایل‌های آپلود شده
    /// </summary>
    [HttpGet]
    public IActionResult GetUploadedFiles([FromQuery] string type = "all")
    {
        try
        {
            var result = new Dictionary<string, List<object>>();

            var types = type.ToLower() == "all" 
                ? new[] { "backgrounds", "audio", "videos" }
                : new[] { type.ToLower() };

            foreach (var fileType in types)
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", fileType);
                var files = new List<object>();

                if (Directory.Exists(uploadPath))
                {
                    var fileInfos = new DirectoryInfo(uploadPath).GetFiles();
                    
                    foreach (var fileInfo in fileInfos)
                    {
                        var baseUrl = $"{Request.Scheme}://{Request.Host}";
                        var fileUrl = $"{baseUrl}/uploads/{fileType}/{fileInfo.Name}";

                        files.Add(new {
                            fileName = fileInfo.Name,
                            url = fileUrl,
                            size = fileInfo.Length,
                            createdAt = fileInfo.CreationTime,
                            type = fileType.TrimEnd('s') // remove 's' from plural
                        });
                    }
                }

                result[fileType] = files;
            }

            return Ok(new { 
                success = true, 
                data = result,
                message = "لیست فایل‌ها با موفقیت دریافت شد" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting uploaded files");
            return StatusCode(500, new { 
                success = false, 
                message = "خطای داخلی سرور", 
                error = ex.Message 
            });
        }
    }
}

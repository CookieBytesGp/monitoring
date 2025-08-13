using Microsoft.AspNetCore.Mvc;
using Monitoring.Application.Interfaces.Camera;
using Monitoring.Application.DTOs.Camera;
using FluentResults;
using AutoMapper;

namespace Monitoring.ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CameraController : ControllerBase
{
    private readonly ICameraService _cameraService;
    private readonly ILogger<CameraController> _logger;
    private readonly IMapper _mapper;

    public CameraController(ICameraService cameraService, ILogger<CameraController> logger, IMapper mapper)
    {
        _cameraService = cameraService;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// دریافت لیست تمام دوربین‌ها
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCameras()
    {
        try
        {
            var result = await _cameraService.GetAllCamerasAsync();
            
            if (result.IsFailed)
                return BadRequest(result.Errors.Select(e => e.Message));
            
            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all cameras");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// دریافت دوربین با ID مشخص
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCameraById(Guid id)
    {
        try
        {
            var result = await _cameraService.GetCameraByIdAsync(id);
            
            if (result.IsFailed)
                return NotFound(result.Errors.Select(e => e.Message));
            
            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting camera {CameraId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// ایجاد دوربین جدید
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCamera([FromBody] CreateCameraDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Creating camera with Name: {Name}, Location: {Location}, IpAddress: {IpAddress}", 
                createDto.Name, createDto.Location, createDto.IpAddress);

            // Map CreateCameraDto to CameraDto using AutoMapper
            var cameraDto = _mapper.Map<CameraDto>(createDto);

            var result = await _cameraService.CreateCameraAsync(cameraDto);
            
            if (result.IsFailed)
            {
                _logger.LogError("Failed to create camera: {Errors}", string.Join(", ", result.Errors.Select(e => e.Message)));
                return BadRequest(result.Errors.Select(e => e.Message));
            }
            
            _logger.LogInformation("Camera created successfully with Id: {CameraId}", result.Value.Id);
            return CreatedAtAction(nameof(GetCameraById), new { id = result.Value.Id }, result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating camera");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// تست اتصال دوربین
    /// </summary>
    [HttpPost("{id}/test-connection")]
    public async Task<IActionResult> TestCameraConnection(Guid id)
    {
        try
        {
            var result = await _cameraService.TestCameraConnectionAsync(id);
            
            if (result.IsFailed)
                return BadRequest(result.Errors.Select(e => e.Message));
            
            return Ok(new { isConnected = result.Value, message = result.Value ? "Camera is reachable" : "Camera is not reachable" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while testing camera connection {CameraId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// اتصال با بهترین استراتژی
    /// </summary>
    [HttpPost("{id}/connect")]
    public async Task<IActionResult> ConnectWithBestStrategy(Guid id)
    {
        try
        {
            var result = await _cameraService.ConnectWithBestStrategyAsync(id);
            
            if (result.IsFailed)
                return BadRequest(result.Errors.Select(e => e.Message));
            
            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while connecting to camera {CameraId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// دریافت استراتژی‌های پشتیبانی شده
    /// </summary>
    [HttpGet("{id}/supported-strategies")]
    public async Task<IActionResult> GetSupportedStrategies(Guid id)
    {
        try
        {
            var result = await _cameraService.GetSupportedStrategiesAsync(id);
            
            if (result.IsFailed)
                return BadRequest(result.Errors.Select(e => e.Message));
            
            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting supported strategies for camera {CameraId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// تست تمام استراتژی‌ها
    /// </summary>
    [HttpPost("{id}/test-all-strategies")]
    public async Task<IActionResult> TestAllStrategies(Guid id)
    {
        try
        {
            var result = await _cameraService.TestAllStrategiesAsync(id);
            
            if (result.IsFailed)
                return BadRequest(result.Errors.Select(e => e.Message));
            
            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while testing all strategies for camera {CameraId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// گرفتن عکس از دوربین
    /// </summary>
    [HttpGet("{id}/snapshot")]
    public async Task<IActionResult> CaptureSnapshot(Guid id)
    {
        try
        {
            var result = await _cameraService.CaptureSnapshotWithBestStrategyAsync(id);
            
            if (result.IsFailed)
                return BadRequest(result.Errors.Select(e => e.Message));
            
            return File(result.Value, "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while capturing snapshot from camera {CameraId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// دریافت URL استریم دوربین
    /// </summary>
    [HttpGet("{id}/stream")]
    public async Task<IActionResult> GetStreamUrl(Guid id)
    {
        try
        {
            var result = await _cameraService.GetCameraStreamAsync(id);
            
            if (result.IsFailed)
                return BadRequest(result.Errors.Select(e => e.Message));
            
            return Ok(new { streamUrl = result.Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting stream URL for camera {CameraId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// تست اتصال دوربین گوشی
    /// </summary>
    [HttpPost("test-phone-camera")]
    public async Task<IActionResult> TestPhoneCamera([FromBody] TestPhoneCameraRequest request)
    {
        try
        {
            _logger.LogInformation("Testing phone camera connection to {Url}", request.StreamUrl);
            
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            // Test snapshot first
            try
            {
                var response = await httpClient.GetAsync(request.SnapshotUrl ?? request.StreamUrl);
                var isSnapshot = response.IsSuccessStatusCode;
                
                return Ok(new
                {
                    IsConnected = isSnapshot,
                    StreamUrl = request.StreamUrl,
                    SnapshotUrl = request.SnapshotUrl,
                    StatusCode = (int)response.StatusCode,
                    ContentType = response.Content.Headers.ContentType?.ToString(),
                    Message = isSnapshot ? "Connection successful!" : "Connection failed"
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    IsConnected = false,
                    StreamUrl = request.StreamUrl,
                    SnapshotUrl = request.SnapshotUrl,
                    Error = ex.Message,
                    Message = "Connection failed"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while testing phone camera");
            return StatusCode(500, "Internal server error");
        }
    }
}

/// <summary>
/// درخواست تست دوربین گوشی
/// </summary>
public class TestPhoneCameraRequest
{
    public string StreamUrl { get; set; } = string.Empty;
    public string? SnapshotUrl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}

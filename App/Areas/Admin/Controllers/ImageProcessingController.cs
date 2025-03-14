 using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Services.Interfaces;
using SixLabors.ImageSharp;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class ImageProcessingController : Controller
    {
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IMotionAnalyticsService _analyticsService;
        private readonly ILoggingService _loggingService;

        public ImageProcessingController(
            IImageProcessingService imageProcessingService,
            IMotionAnalyticsService analyticsService,
            ILoggingService loggingService)
        {
            _imageProcessingService = imageProcessingService;
            _analyticsService = analyticsService;
            _loggingService = loggingService;
        }

        [HttpPost]
        public async Task<IActionResult> EnhanceImage(int eventId, float brightness = 0, float contrast = 0, float sharpness = 0)
        {
            try
            {
                var motionEvent = await _analyticsService.GetEventByIdAsync(eventId);
                if (motionEvent == null || string.IsNullOrEmpty(motionEvent.ImagePath))
                {
                    return NotFound();
                }

                var enhancedPath = await _imageProcessingService.EnhanceImageAsync(
                    motionEvent.ImagePath,
                    brightness,
                    contrast,
                    sharpness);

                return Json(new { success = true, path = enhancedPath });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to enhance image", ex.Message);
                return StatusCode(500, "Failed to process image");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResizeImage(int eventId, int width, int height, bool maintainAspectRatio = true)
        {
            try
            {
                var motionEvent = await _analyticsService.GetEventByIdAsync(eventId);
                if (motionEvent == null || string.IsNullOrEmpty(motionEvent.ImagePath))
                {
                    return NotFound();
                }

                var resizedPath = await _imageProcessingService.ResizeImageAsync(
                    motionEvent.ImagePath,
                    width,
                    height,
                    maintainAspectRatio);

                return Json(new { success = true, path = resizedPath });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to resize image", ex.Message);
                return StatusCode(500, "Failed to process image");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CropImage(int eventId, int x, int y, int width, int height)
        {
            try
            {
                var motionEvent = await _analyticsService.GetEventByIdAsync(eventId);
                if (motionEvent == null || string.IsNullOrEmpty(motionEvent.ImagePath))
                {
                    return NotFound();
                }

                var croppedPath = await _imageProcessingService.CropImageAsync(
                    motionEvent.ImagePath,
                    new SixLabors.ImageSharp.Rectangle(x, y, width, height));

                return Json(new { success = true, path = croppedPath });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to crop image", ex.Message);
                return StatusCode(500, "Failed to process image");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAnnotation(
            int eventId, 
            string text, 
            int x, 
            int y, 
            float fontSize = 16, 
            string color = "#FF0000")
        {
            try
            {
                var motionEvent = await _analyticsService.GetEventByIdAsync(eventId);
                if (motionEvent == null || string.IsNullOrEmpty(motionEvent.ImagePath))
                {
                    return NotFound();
                }

                var annotatedPath = await _imageProcessingService.AddAnnotationAsync(
                    motionEvent.ImagePath,
                    text,
                    new SixLabors.ImageSharp.Point(x, y),
                    fontSize,
                    color);

                return Json(new { success = true, path = annotatedPath });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to add annotation", ex.Message);
                return StatusCode(500, "Failed to process image");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DrawRectangle(
            int eventId, 
            int x, 
            int y, 
            int width, 
            int height, 
            float thickness = 2, 
            string color = "#FF0000")
        {
            try
            {
                var motionEvent = await _analyticsService.GetEventByIdAsync(eventId);
                if (motionEvent == null || string.IsNullOrEmpty(motionEvent.ImagePath))
                {
                    return NotFound();
                }

                var processedPath = await _imageProcessingService.DrawRectangleAsync(
                    motionEvent.ImagePath,
                    new SixLabors.ImageSharp.Rectangle(x, y, width, height),
                    thickness,
                    color);

                return Json(new { success = true, path = processedPath });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to draw rectangle", ex.Message);
                return StatusCode(500, "Failed to process image");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RotateImage(int eventId, float degrees)
        {
            try
            {
                var motionEvent = await _analyticsService.GetEventByIdAsync(eventId);
                if (motionEvent == null || string.IsNullOrEmpty(motionEvent.ImagePath))
                {
                    return NotFound();
                }

                var rotatedPath = await _imageProcessingService.RotateImageAsync(
                    motionEvent.ImagePath,
                    degrees);

                return Json(new { success = true, path = rotatedPath });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to rotate image", ex.Message);
                return StatusCode(500, "Failed to process image");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AnalyzeQuality(int eventId)
        {
            try
            {
                var motionEvent = await _analyticsService.GetEventByIdAsync(eventId);
                if (motionEvent == null || string.IsNullOrEmpty(motionEvent.ImagePath))
                {
                    return NotFound();
                }

                var (brightness, contrast) = await _imageProcessingService.AnalyzeImageQualityAsync(
                    motionEvent.ImagePath);

                var isBlurry = await _imageProcessingService.DetectBlurAsync(motionEvent.ImagePath);

                return Json(new
                {
                    success = true,
                    brightness,
                    contrast,
                    isBlurry
                });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to analyze image quality", ex.Message);
                return StatusCode(500, "Failed to analyze image");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProcessedImage(string processedImagePath)
        {
            try
            {
                await _imageProcessingService.DeleteProcessedImageAsync(processedImagePath);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to delete processed image", ex.Message);
                return StatusCode(500, "Failed to delete image");
            }
        }
    }
}
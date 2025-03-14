 using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace App.Services.Interfaces
{
    public interface IImageProcessingService
    {
        Task<string> EnhanceImageAsync(string imagePath, float brightness = 0, float contrast = 0, float sharpness = 0);
        
        Task<string> ResizeImageAsync(string imagePath, int width, int height, bool maintainAspectRatio = true);
        
        Task<string> CropImageAsync(string imagePath, SixLabors.ImageSharp.Rectangle region);
        
        Task<string> AddAnnotationAsync(string imagePath, string text, SixLabors.ImageSharp.Point location, float fontSize = 16, string color = "#FF0000");
        
        Task<string> DrawRectangleAsync(string imagePath, SixLabors.ImageSharp.Rectangle region, float thickness = 2, string color = "#FF0000");
        
        Task<string> RotateImageAsync(string imagePath, float degrees);
        
        Task<(float Brightness, float Contrast)> AnalyzeImageQualityAsync(string imagePath);
        
        Task<bool> DetectBlurAsync(string imagePath, double threshold = 100.0);
        
        Task<string> ApplyMotionMaskAsync(string imagePath, string previousImagePath, double threshold = 0.1);
        
        Task<string> SaveProcessedImageAsync(Image<Rgba32> image, string originalPath, string suffix);
        
        Task DeleteProcessedImageAsync(string processedImagePath);
    }
}
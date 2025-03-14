using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using App.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using System.Numerics;

namespace App.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly ILoggingService _loggingService;
        private readonly string _processedImagePath;
        private readonly FontFamily _defaultFont;

        public ImageProcessingService(
            ILoggingService loggingService,
            string processedImagePath = "wwwroot/processed_images")
        {
            _loggingService = loggingService;
            _processedImagePath = processedImagePath;
            _defaultFont = SystemFonts.Get("Arial");

            if (!Directory.Exists(_processedImagePath))
            {
                Directory.CreateDirectory(_processedImagePath);
            }
        }

        public async Task<string> EnhanceImageAsync(string imagePath, float brightness = 0, float contrast = 0, float sharpness = 0)
        {
            try
            {
                using var image = await Image.LoadAsync<Rgba32>(imagePath);
                
                image.Mutate(x => x
                    .Brightness(1 + brightness)
                    .Contrast(1 + contrast));

                if (sharpness > 0)
                {
                    image.Mutate(x => x.GaussianSharpen(sharpness));
                }

                return await SaveProcessedImageAsync(image, imagePath, "enhanced");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to enhance image", ex.Message);
                throw;
            }
        }

        public async Task<string> ResizeImageAsync(string imagePath, int width, int height, bool maintainAspectRatio = true)
        {
            try
            {
                using var image = await Image.LoadAsync<Rgba32>(imagePath);
                
                if (maintainAspectRatio)
                {
                    var resizeOptions = new ResizeOptions
                    {
                        Size = new Size(width, height),
                        Mode = ResizeMode.Max
                    };
                    image.Mutate(x => x.Resize(resizeOptions));
                }
                else
                {
                    image.Mutate(x => x.Resize(width, height));
                }

                return await SaveProcessedImageAsync(image, imagePath, "resized");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to resize image", ex.Message);
                throw;
            }
        }

        public async Task<string> CropImageAsync(string imagePath, SixLabors.ImageSharp.Rectangle cropArea)
        {
            try
            {
                using var image = await Image.LoadAsync<Rgba32>(imagePath);
                image.Mutate(ctx => ctx.Crop(new SixLabors.ImageSharp.Rectangle(
                    cropArea.X,
                    cropArea.Y,
                    Math.Min(cropArea.Width, image.Width - cropArea.X),
                    Math.Min(cropArea.Height, image.Height - cropArea.Y)
                )));

                return await SaveProcessedImageAsync(image, imagePath, "cropped");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to crop image", ex.Message);
                throw;
            }
        }

        public async Task<string> AddAnnotationAsync(string imagePath, string text, SixLabors.ImageSharp.Point location, float fontSize = 16, string color = "#FF0000")
        {
            try
            {
                using var image = await Image.LoadAsync<Rgba32>(imagePath);
                var font = new Font(_defaultFont, fontSize);
                var textColor = Color.ParseHex(color);

                image.Mutate(x => x.DrawText(text, font, textColor, location));
                return await SaveProcessedImageAsync(image, imagePath, "annotated");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to add annotation", ex.Message);
                throw;
            }
        }

        public async Task<string> DrawRectangleAsync(string imagePath, SixLabors.ImageSharp.Rectangle rect, float thickness = 2.0f, string color = "#FF0000")
        {
            try
            {
                using var image = await Image.LoadAsync<Rgba32>(imagePath);
                var pen = CreatePen(color, thickness);

                image.Mutate(x => x.Draw(pen, rect));

                return await SaveProcessedImageAsync(image, imagePath, "marked");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to draw rectangle", ex.Message);
                throw;
            }
        }

        public async Task<string> RotateImageAsync(string imagePath, float degrees)
        {
            try
            {
                using var image = await Image.LoadAsync<Rgba32>(imagePath);
                image.Mutate(x => x.Rotate(degrees));
                return await SaveProcessedImageAsync(image, imagePath, "rotated");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to rotate image", ex.Message);
                throw;
            }
        }

        public async Task<(float Brightness, float Contrast)> AnalyzeImageQualityAsync(string imagePath)
        {
            try
            {
                using var image = await Image.LoadAsync<Rgba32>(imagePath);
                float totalBrightness = 0;
                float totalContrast = 0;
                int pixelCount = image.Width * image.Height;

                // Calculate average brightness
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = image[x, y];
                        totalBrightness += (pixel.R + pixel.G + pixel.B) / (3.0f * 255.0f);
                    }
                }

                float averageBrightness = totalBrightness / pixelCount;

                // Calculate contrast (standard deviation)
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = image[x, y];
                        float pixelBrightness = (pixel.R + pixel.G + pixel.B) / (3.0f * 255.0f);
                        totalContrast += (float)Math.Pow(pixelBrightness - averageBrightness, 2);
                    }
                }

                float contrast = (float)Math.Sqrt(totalContrast / pixelCount);

                return (averageBrightness, contrast);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to analyze image quality", ex.Message);
                throw;
            }
        }

        public async Task<bool> DetectBlurAsync(string imagePath, double threshold = 100.0)
        {
            try
            {
                using var image = await Image.LoadAsync<Rgba32>(imagePath);
                double laplacianVariance = 0;

                // Convert to grayscale and apply Laplacian operator
                image.Mutate(x => x
                    .Grayscale()
                    .GaussianBlur(1.0f));

                // Calculate Laplacian variance
                // A lower variance indicates a blurrier image
                for (int y = 1; y < image.Height - 1; y++)
                {
                    for (int x = 1; x < image.Width - 1; x++)
                    {
                        var pixel = image[x, y];
                        var laplacian = -4 * pixel.R +
                            image[x + 1, y].R + image[x - 1, y].R +
                            image[x, y + 1].R + image[x, y - 1].R;
                        laplacianVariance += laplacian * laplacian;
                    }
                }

                laplacianVariance /= (image.Width - 2) * (image.Height - 2);
                return laplacianVariance < threshold;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to detect blur", ex.Message);
                throw;
            }
        }

        public async Task<string> ApplyMotionMaskAsync(string imagePath, string previousImagePath, double threshold = 0.1)
        {
            try
            {
                using var currentImage = await Image.LoadAsync<Rgba32>(imagePath);
                using var previousImage = await Image.LoadAsync<Rgba32>(previousImagePath);

                if (currentImage.Size != previousImage.Size)
                {
                    throw new ArgumentException("Images must be the same size");
                }

                var motionMask = new Image<Rgba32>(currentImage.Width, currentImage.Height);

                // Calculate difference between frames
                for (int y = 0; y < currentImage.Height; y++)
                {
                    for (int x = 0; x < currentImage.Width; x++)
                    {
                        var current = currentImage[x, y];
                        var previous = previousImage[x, y];

                        var diff = Math.Abs(current.R - previous.R) +
                                 Math.Abs(current.G - previous.G) +
                                 Math.Abs(current.B - previous.B);

                        if (diff / (3.0 * 255.0) > threshold)
                        {
                            motionMask[x, y] = new Rgba32(255, 0, 0, 128); // Semi-transparent red
                        }
                        else
                        {
                            motionMask[x, y] = new Rgba32(0, 0, 0, 0); // Transparent
                        }
                    }
                }

                // Overlay motion mask on current image
                currentImage.Mutate(x => x.DrawImage(motionMask, 1.0f));
                return await SaveProcessedImageAsync(currentImage, imagePath, "motion");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to apply motion mask", ex.Message);
                throw;
            }
        }

        public async Task<string> SaveProcessedImageAsync(Image<Rgba32> image, string originalPath, string suffix)
        {
            var fileName = $"{System.IO.Path.GetFileNameWithoutExtension(originalPath)}_{suffix}_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
            var savePath = System.IO.Path.Combine(_processedImagePath, fileName);

            await image.SaveAsJpegAsync(savePath);
            return savePath;
        }

        public async Task DeleteProcessedImageAsync(string processedImagePath)
        {
            try
            {
                if (File.Exists(processedImagePath))
                {
                    File.Delete(processedImagePath);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex, "Failed to delete processed image", ex.Message);
                throw;
            }
        }

        private Pen CreatePen(string color, float thickness)
        {
            return Pens.Solid(Color.ParseHex(color), thickness);
        }

        private void DrawMotionRegions(Image image, string color, float thickness, List<SixLabors.ImageSharp.Rectangle> regions)
        {
            var pen = CreatePen(color, thickness);
            
            image.Mutate(x => 
            {
                foreach (var region in regions)
                {
                    x.Draw(pen, region);
                }
            });
        }
    }
}
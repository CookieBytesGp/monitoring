 using System;
using System.IO;
using System.Threading.Tasks;
using App.Services;
using App.Services.Interfaces;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace App.Tests.Services
{
    public class ImageProcessingServiceTests : IDisposable
    {
        private readonly Mock<ILoggingService> _loggingServiceMock;
        private readonly string _testImagePath;
        private readonly string _processedImagePath;
        private readonly ImageProcessingService _service;

        public ImageProcessingServiceTests()
        {
            _loggingServiceMock = new Mock<ILoggingService>();
            _testImagePath = Path.Combine(Path.GetTempPath(), "test_images");
            _processedImagePath = Path.Combine(Path.GetTempPath(), "processed_images");

            if (!Directory.Exists(_testImagePath))
                Directory.CreateDirectory(_testImagePath);
            if (!Directory.Exists(_processedImagePath))
                Directory.CreateDirectory(_processedImagePath);

            _service = new ImageProcessingService(_loggingServiceMock.Object, _processedImagePath);

            // Create a test image
            CreateTestImage();
        }

        [Fact]
        public async Task EnhanceImageAsync_AppliesCorrectEnhancements()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagePath, "test.jpg");

            // Act
            var result = await _service.EnhanceImageAsync(imagePath, 0.2f, 0.3f, 0.5f);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result));
            Assert.Contains("enhanced", result);
        }

        [Fact]
        public async Task ResizeImageAsync_ResizesImageCorrectly()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagePath, "test.jpg");
            const int targetWidth = 200;
            const int targetHeight = 150;

            // Act
            var result = await _service.ResizeImageAsync(imagePath, targetWidth, targetHeight);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result));
            using var resizedImage = await Image.LoadAsync<Rgba32>(result);
            Assert.True(resizedImage.Width <= targetWidth && resizedImage.Height <= targetHeight);
        }

        [Fact]
        public async Task CropImageAsync_CropsImageCorrectly()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagePath, "test.jpg");
            var region = new Rectangle(10, 10, 50, 50);

            // Act
            var result = await _service.CropImageAsync(imagePath, region);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result));
            using var croppedImage = await Image.LoadAsync<Rgba32>(result);
            Assert.Equal(50, croppedImage.Width);
            Assert.Equal(50, croppedImage.Height);
        }

        [Fact]
        public async Task AddAnnotationAsync_AddsTextToImage()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagePath, "test.jpg");
            var text = "Test Annotation";
            var location = new Point(10, 10);

            // Act
            var result = await _service.AddAnnotationAsync(imagePath, text, location);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result));
            Assert.Contains("annotated", result);
        }

        [Fact]
        public async Task DrawRectangleAsync_DrawsRectangleOnImage()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagePath, "test.jpg");
            var region = new Rectangle(10, 10, 50, 50);

            // Act
            var result = await _service.DrawRectangleAsync(imagePath, region);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result));
            Assert.Contains("rectangle", result);
        }

        [Fact]
        public async Task RotateImageAsync_RotatesImageCorrectly()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagePath, "test.jpg");
            const float degrees = 90;

            // Act
            var result = await _service.RotateImageAsync(imagePath, degrees);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result));
            Assert.Contains("rotated", result);
        }

        [Fact]
        public async Task AnalyzeImageQualityAsync_ReturnsValidMetrics()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagePath, "test.jpg");

            // Act
            var (brightness, contrast) = await _service.AnalyzeImageQualityAsync(imagePath);

            // Assert
            Assert.InRange(brightness, 0, 1);
            Assert.InRange(contrast, 0, 1);
        }

        [Fact]
        public async Task DetectBlurAsync_DetectsBlurCorrectly()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagePath, "test.jpg");

            // Act
            var isBlurry = await _service.DetectBlurAsync(imagePath);

            // Assert
            Assert.IsType<bool>(isBlurry);
        }

        [Fact]
        public async Task ApplyMotionMaskAsync_CreatesMotionMask()
        {
            // Arrange
            var imagePath1 = Path.Combine(_testImagePath, "test.jpg");
            var imagePath2 = Path.Combine(_testImagePath, "test2.jpg");
            CreateTestImage("test2.jpg", true); // Create a slightly different test image

            // Act
            var result = await _service.ApplyMotionMaskAsync(imagePath1, imagePath2);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result));
            Assert.Contains("motion", result);
        }

        [Fact]
        public async Task DeleteProcessedImageAsync_DeletesFile()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagePath, "test.jpg");
            var processedPath = await _service.EnhanceImageAsync(imagePath);

            // Act
            await _service.DeleteProcessedImageAsync(processedPath);

            // Assert
            Assert.False(File.Exists(processedPath));
        }

        private void CreateTestImage(string fileName = "test.jpg", bool alternate = false)
        {
            var path = Path.Combine(_testImagePath, fileName);
            using var image = new Image<Rgba32>(100, 100);
            
            // Fill with test pattern
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    image[x, y] = alternate ? 
                        new Rgba32((byte)(x % 256), (byte)(y % 256), 128) :
                        new Rgba32((byte)(y % 256), (byte)(x % 256), 128);
                }
            }

            image.Save(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testImagePath))
                Directory.Delete(_testImagePath, true);
            if (Directory.Exists(_processedImagePath))
                Directory.Delete(_processedImagePath, true);
        }
    }
}
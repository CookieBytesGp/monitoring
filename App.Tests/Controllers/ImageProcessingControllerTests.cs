 using System;
using System.Threading.Tasks;
using App.Areas.Admin.Controllers;
using App.Models;
using App.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SixLabors.ImageSharp;
using Xunit;

namespace App.Tests.Controllers
{
    public class ImageProcessingControllerTests
    {
        private readonly Mock<IImageProcessingService> _imageProcessingServiceMock;
        private readonly Mock<IMotionAnalyticsService> _analyticsServiceMock;
        private readonly Mock<ILoggingService> _loggingServiceMock;
        private readonly ImageProcessingController _controller;
        private readonly MotionEvent _testEvent;

        public ImageProcessingControllerTests()
        {
            _imageProcessingServiceMock = new Mock<IImageProcessingService>();
            _analyticsServiceMock = new Mock<IMotionAnalyticsService>();
            _loggingServiceMock = new Mock<ILoggingService>();

            _controller = new ImageProcessingController(
                _imageProcessingServiceMock.Object,
                _analyticsServiceMock.Object,
                _loggingServiceMock.Object
            );

            _testEvent = new MotionEvent
            {
                Id = 1,
                CameraId = 1,
                ImagePath = "/path/to/test/image.jpg"
            };
        }

        [Fact]
        public async Task EnhanceImage_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((MotionEvent)null);

            // Act
            var result = await _controller.EnhanceImage(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EnhanceImage_ReturnsJsonResult_WhenSuccessful()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(1))
                .ReturnsAsync(_testEvent);
            _imageProcessingServiceMock.Setup(x => x.EnhanceImageAsync(
                    It.IsAny<string>(),
                    It.IsAny<float>(),
                    It.IsAny<float>(),
                    It.IsAny<float>()))
                .ReturnsAsync("/path/to/enhanced/image.jpg");

            // Act
            var result = await _controller.EnhanceImage(1, 0.2f, 0.3f, 0.5f);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic value = jsonResult.Value;
            Assert.True((bool)value.success);
            Assert.Equal("/path/to/enhanced/image.jpg", (string)value.path);
        }

        [Fact]
        public async Task ResizeImage_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((MotionEvent)null);

            // Act
            var result = await _controller.ResizeImage(1, 100, 100);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ResizeImage_ReturnsJsonResult_WhenSuccessful()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(1))
                .ReturnsAsync(_testEvent);
            _imageProcessingServiceMock.Setup(x => x.ResizeImageAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .ReturnsAsync("/path/to/resized/image.jpg");

            // Act
            var result = await _controller.ResizeImage(1, 100, 100);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic value = jsonResult.Value;
            Assert.True((bool)value.success);
            Assert.Equal("/path/to/resized/image.jpg", (string)value.path);
        }

        [Fact]
        public async Task CropImage_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((MotionEvent)null);

            // Act
            var result = await _controller.CropImage(1, 0, 0, 100, 100);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CropImage_ReturnsJsonResult_WhenSuccessful()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(1))
                .ReturnsAsync(_testEvent);
            _imageProcessingServiceMock.Setup(x => x.CropImageAsync(
                    It.IsAny<string>(),
                    It.IsAny<Rectangle>()))
                .ReturnsAsync("/path/to/cropped/image.jpg");

            // Act
            var result = await _controller.CropImage(1, 0, 0, 100, 100);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic value = jsonResult.Value;
            Assert.True((bool)value.success);
            Assert.Equal("/path/to/cropped/image.jpg", (string)value.path);
        }

        [Fact]
        public async Task AddAnnotation_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((MotionEvent)null);

            // Act
            var result = await _controller.AddAnnotation(1, "Test", 0, 0);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddAnnotation_ReturnsJsonResult_WhenSuccessful()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(1))
                .ReturnsAsync(_testEvent);
            _imageProcessingServiceMock.Setup(x => x.AddAnnotationAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Point>(),
                    It.IsAny<float>(),
                    It.IsAny<string>()))
                .ReturnsAsync("/path/to/annotated/image.jpg");

            // Act
            var result = await _controller.AddAnnotation(1, "Test", 0, 0);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic value = jsonResult.Value;
            Assert.True((bool)value.success);
            Assert.Equal("/path/to/annotated/image.jpg", (string)value.path);
        }

        [Fact]
        public async Task AnalyzeQuality_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((MotionEvent)null);

            // Act
            var result = await _controller.AnalyzeQuality(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AnalyzeQuality_ReturnsJsonResult_WhenSuccessful()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(1))
                .ReturnsAsync(_testEvent);
            _imageProcessingServiceMock.Setup(x => x.AnalyzeImageQualityAsync(It.IsAny<string>()))
                .ReturnsAsync((0.5f, 0.7f));
            _imageProcessingServiceMock.Setup(x => x.DetectBlurAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.AnalyzeQuality(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic value = jsonResult.Value;
            Assert.True((bool)value.success);
            Assert.Equal(0.5f, (float)value.brightness);
            Assert.Equal(0.7f, (float)value.contrast);
            Assert.False((bool)value.isBlurry);
        }

        [Fact]
        public async Task DeleteProcessedImage_ReturnsJsonResult_WhenSuccessful()
        {
            // Arrange
            _imageProcessingServiceMock.Setup(x => x.DeleteProcessedImageAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteProcessedImage("/path/to/processed/image.jpg");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic value = jsonResult.Value;
            Assert.True((bool)value.success);
        }

        [Fact]
        public async Task DeleteProcessedImage_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            _imageProcessingServiceMock.Setup(x => x.DeleteProcessedImageAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.DeleteProcessedImage("/path/to/processed/image.jpg");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
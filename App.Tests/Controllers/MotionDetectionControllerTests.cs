 using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using App.Areas.Admin.Controllers;
using App.Services.Interfaces;
using App.Models;
using System.Collections.Generic;

namespace App.Tests.Controllers
{
    public class MotionDetectionControllerTests
    {
        private readonly Mock<IMotionDetectionService> _motionDetectionServiceMock;
        private readonly Mock<ICameraService> _cameraServiceMock;
        private readonly Mock<ILoggingService> _loggingServiceMock;
        private readonly MotionDetectionController _controller;

        public MotionDetectionControllerTests()
        {
            _motionDetectionServiceMock = new Mock<IMotionDetectionService>();
            _cameraServiceMock = new Mock<ICameraService>();
            _loggingServiceMock = new Mock<ILoggingService>();

            _controller = new MotionDetectionController(
                _motionDetectionServiceMock.Object,
                _cameraServiceMock.Object,
                _loggingServiceMock.Object
            );
        }

        [Fact]
        public async Task Index_ReturnsViewWithCameras()
        {
            // Arrange
            var cameras = new List<CameraDevice>
            {
                new CameraDevice { Id = "1", Name = "Camera 1" },
                new CameraDevice { Id = "2", Name = "Camera 2" }
            };
            _cameraServiceMock.Setup(s => s.GetAllCamerasAsync())
                             .ReturnsAsync(cameras);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CameraDevice>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Configure_ReturnsNotFound_WhenCameraDoesNotExist()
        {
            // Arrange
            string cameraId = "non-existent";
            _cameraServiceMock.Setup(s => s.GetCameraByIdAsync(cameraId))
                             .ReturnsAsync((CameraDevice)null);

            // Act
            var result = await _controller.Configure(cameraId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Configure_ReturnsViewWithSettings_WhenCameraExists()
        {
            // Arrange
            string cameraId = "test-camera";
            var camera = new CameraDevice { Id = cameraId, Name = "Test Camera" };
            var settings = new MotionDetectionSettings { IsActive = true, Sensitivity = 0.5 };

            _cameraServiceMock.Setup(s => s.GetCameraByIdAsync(cameraId))
                             .ReturnsAsync(camera);
            _motionDetectionServiceMock.Setup(s => s.GetSettingsAsync(cameraId))
                                     .ReturnsAsync(settings);

            // Act
            var result = await _controller.Configure(cameraId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MotionDetectionSettings>(viewResult.Model);
            Assert.Equal(settings.IsActive, model.IsActive);
            Assert.Equal(settings.Sensitivity, model.Sensitivity);
            Assert.Equal(camera, viewResult.ViewData["Camera"]);
        }

        [Fact]
        public async Task UpdateSettings_ReturnsSuccess_WhenUpdateSucceeds()
        {
            // Arrange
            string cameraId = "test-camera";
            var settings = new MotionDetectionSettings
            {
                IsActive = true,
                Sensitivity = 0.7,
                RegionOfInterest = new Rectangle { X = 0, Y = 0, Width = 100, Height = 100 }
            };

            // Act
            var result = await _controller.UpdateSettings(cameraId, settings);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = Assert.IsType<object>(jsonResult.Value);
            Assert.True((bool)value.GetType().GetProperty("success").GetValue(value));

            // Verify service calls
            _motionDetectionServiceMock.Verify(s => s.UpdateSensitivityAsync(cameraId, settings.Sensitivity), Times.Once);
            _motionDetectionServiceMock.Verify(s => s.UpdateRegionOfInterestAsync(cameraId, settings.RegionOfInterest), Times.Once);
            _motionDetectionServiceMock.Verify(s => s.StartDetectionAsync(cameraId), Times.Once);
        }

        [Fact]
        public async Task UpdateSettings_ReturnsFailure_WhenExceptionOccurs()
        {
            // Arrange
            string cameraId = "test-camera";
            var settings = new MotionDetectionSettings { IsActive = true };
            _motionDetectionServiceMock.Setup(s => s.UpdateSensitivityAsync(cameraId, It.IsAny<double>()))
                                     .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UpdateSettings(cameraId, settings);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = Assert.IsType<object>(jsonResult.Value);
            Assert.False((bool)value.GetType().GetProperty("success").GetValue(value));
            
            // Verify error logging
            _loggingServiceMock.Verify(
                l => l.LogErrorAsync(
                    It.IsAny<Exception>(),
                    "MotionDetection",
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task ToggleDetection_ReturnsSuccess_WhenToggleSucceeds()
        {
            // Arrange
            string cameraId = "test-camera";
            bool enable = true;

            // Act
            var result = await _controller.ToggleDetection(cameraId, enable);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = Assert.IsType<object>(jsonResult.Value);
            Assert.True((bool)value.GetType().GetProperty("success").GetValue(value));

            // Verify service call
            _motionDetectionServiceMock.Verify(s => s.StartDetectionAsync(cameraId), Times.Once);
        }

        [Fact]
        public async Task GetStatus_ReturnsCorrectStatus()
        {
            // Arrange
            string cameraId = "test-camera";
            bool isActive = true;
            var settings = new MotionDetectionSettings { IsActive = true, Sensitivity = 0.5 };

            _motionDetectionServiceMock.Setup(s => s.IsDetectionActiveAsync(cameraId))
                                     .ReturnsAsync(isActive);
            _motionDetectionServiceMock.Setup(s => s.GetSettingsAsync(cameraId))
                                     .ReturnsAsync(settings);

            // Act
            var result = await _controller.GetStatus(cameraId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic value = jsonResult.Value;
            Assert.Equal(isActive, value.isActive);
            Assert.Equal(settings, value.settings);
        }
    }
}
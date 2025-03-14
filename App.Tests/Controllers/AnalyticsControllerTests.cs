 using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Areas.Admin.Controllers;
using App.Models;
using App.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace App.Tests.Controllers
{
    public class AnalyticsControllerTests
    {
        private readonly Mock<IMotionAnalyticsService> _analyticsServiceMock;
        private readonly Mock<ICameraService> _cameraServiceMock;
        private readonly AnalyticsController _controller;

        public AnalyticsControllerTests()
        {
            _analyticsServiceMock = new Mock<IMotionAnalyticsService>();
            _cameraServiceMock = new Mock<ICameraService>();
            _controller = new AnalyticsController(_analyticsServiceMock.Object, _cameraServiceMock.Object);
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
            _cameraServiceMock.Setup(x => x.GetAllCamerasAsync())
                .ReturnsAsync(cameras);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CameraDevice>>(viewResult.Model);
            Assert.Equal(2, ((List<CameraDevice>)model).Count);
        }

        [Fact]
        public async Task CameraAnalytics_ReturnsNotFound_WhenCameraDoesNotExist()
        {
            // Arrange
            _cameraServiceMock.Setup(x => x.GetCameraByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((CameraDevice)null);

            // Act
            var result = await _controller.CameraAnalytics("nonexistent");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CameraAnalytics_ReturnsViewWithCamera_WhenCameraExists()
        {
            // Arrange
            var camera = new CameraDevice { Id = "1", Name = "Camera 1" };
            _cameraServiceMock.Setup(x => x.GetCameraByIdAsync("1"))
                .ReturnsAsync(camera);

            // Act
            var result = await _controller.CameraAnalytics("1");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(camera, viewResult.ViewData["Camera"]);
        }

        [Fact]
        public async Task GetAnalytics_ReturnsJsonResult()
        {
            // Arrange
            var analytics = new
            {
                TotalEvents = 10,
                AverageMotionPercentage = 0.5f,
                PeakMotionPercentage = 0.8f
            };
            _analyticsServiceMock.Setup(x => x.GetAnalyticsAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .ReturnsAsync(analytics);

            // Act
            var result = await _controller.GetAnalytics("1", DateTime.UtcNow, DateTime.UtcNow);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public async Task GetEventsByHour_ReturnsJsonResult()
        {
            // Arrange
            var events = new Dictionary<int, int>
            {
                { 0, 5 },
                { 1, 3 },
                { 2, 7 }
            };
            _analyticsServiceMock.Setup(x => x.GetEventsByHourAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>()))
                .ReturnsAsync(events);

            // Act
            var result = await _controller.GetEventsByHour("1", DateTime.UtcNow);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public async Task AcknowledgeEvent_ReturnsSuccessResult_WhenSuccessful()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.AcknowledgeEventAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AcknowledgeEvent(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic value = jsonResult.Value;
            Assert.True((bool)value.success);
        }

        [Fact]
        public async Task AcknowledgeEvent_ReturnsFailureResult_WhenExceptionOccurs()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.AcknowledgeEventAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.AcknowledgeEvent(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic value = jsonResult.Value;
            Assert.False((bool)value.success);
        }
    }
}
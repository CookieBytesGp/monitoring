 using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Areas.Admin.Controllers;
using App.Models;
using App.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.IO;
using System.Text;

namespace App.Tests.Controllers
{
    public class ExportControllerTests
    {
        private readonly Mock<IMotionAnalyticsService> _analyticsServiceMock;
        private readonly Mock<ICameraService> _cameraServiceMock;
        private readonly Mock<ILoggingService> _loggingServiceMock;
        private readonly ExportController _controller;

        public ExportControllerTests()
        {
            _analyticsServiceMock = new Mock<IMotionAnalyticsService>();
            _cameraServiceMock = new Mock<ICameraService>();
            _loggingServiceMock = new Mock<ILoggingService>();
            _controller = new ExportController(
                _analyticsServiceMock.Object,
                _cameraServiceMock.Object,
                _loggingServiceMock.Object);
        }

        [Fact]
        public async Task ExportEvents_ReturnsCsvFile_WhenFormatIsCsv()
        {
            // Arrange
            var events = new List<MotionEvent>
            {
                new MotionEvent
                {
                    Id = 1,
                    CameraId = "1",
                    CameraName = "Camera 1",
                    Timestamp = DateTime.UtcNow,
                    MotionPercentage = 0.5f
                }
            };

            _analyticsServiceMock.Setup(x => x.GetMotionEventsAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .ReturnsAsync(events);

            // Act
            var result = await _controller.ExportEvents(format: "csv") as FileResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("text/csv", result.ContentType);
            Assert.EndsWith(".csv", result.FileDownloadName);
        }

        [Fact]
        public async Task ExportEvents_ReturnsJsonFile_WhenFormatIsJson()
        {
            // Arrange
            var events = new List<MotionEvent>
            {
                new MotionEvent
                {
                    Id = 1,
                    CameraId = "1",
                    CameraName = "Camera 1",
                    Timestamp = DateTime.UtcNow,
                    MotionPercentage = 0.5f
                }
            };

            _analyticsServiceMock.Setup(x => x.GetMotionEventsAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .ReturnsAsync(events);

            // Act
            var result = await _controller.ExportEvents(format: "json") as FileResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/json", result.ContentType);
            Assert.EndsWith(".json", result.FileDownloadName);
        }

        [Fact]
        public async Task ExportEvents_ReturnsExcelFile_WhenFormatIsExcel()
        {
            // Arrange
            var events = new List<MotionEvent>
            {
                new MotionEvent
                {
                    Id = 1,
                    CameraId = "1",
                    CameraName = "Camera 1",
                    Timestamp = DateTime.UtcNow,
                    MotionPercentage = 0.5f
                }
            };

            _analyticsServiceMock.Setup(x => x.GetMotionEventsAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .ReturnsAsync(events);

            // Act
            var result = await _controller.ExportEvents(format: "excel") as FileResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.ContentType);
            Assert.EndsWith(".xlsx", result.FileDownloadName);
        }

        [Fact]
        public async Task ExportEvents_ReturnsBadRequest_WhenFormatIsUnsupported()
        {
            // Act
            var result = await _controller.ExportEvents(format: "unsupported");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ExportEvents_ReturnsServerError_WhenExceptionOccurs()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetMotionEventsAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.ExportEvents() as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            _loggingServiceMock.Verify(x => x.LogErrorAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ExportAnalytics_ReturnsCsvFile_WhenFormatIsCsv()
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
            var result = await _controller.ExportAnalytics(format: "csv") as FileResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("text/csv", result.ContentType);
            Assert.EndsWith(".csv", result.FileDownloadName);
        }

        [Fact]
        public async Task ExportAnalytics_ReturnsJsonFile_WhenFormatIsJson()
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
            var result = await _controller.ExportAnalytics(format: "json") as FileResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/json", result.ContentType);
            Assert.EndsWith(".json", result.FileDownloadName);
        }

        [Fact]
        public async Task ExportAnalytics_ReturnsServerError_WhenExceptionOccurs()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetAnalyticsAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.ExportAnalytics() as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            _loggingServiceMock.Verify(x => x.LogErrorAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
        }
    }
}
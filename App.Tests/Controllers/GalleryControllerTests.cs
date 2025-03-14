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
    public class GalleryControllerTests
    {
        private readonly Mock<IMotionAnalyticsService> _analyticsServiceMock;
        private readonly Mock<ICameraService> _cameraServiceMock;
        private readonly GalleryController _controller;

        public GalleryControllerTests()
        {
            _analyticsServiceMock = new Mock<IMotionAnalyticsService>();
            _cameraServiceMock = new Mock<ICameraService>();
            _controller = new GalleryController(_analyticsServiceMock.Object, _cameraServiceMock.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewWithCorrectViewBagData()
        {
            // Arrange
            var cameras = new List<CameraDevice>
            {
                new CameraDevice { Id = "1", Name = "Camera 1" },
                new CameraDevice { Id = "2", Name = "Camera 2" }
            };
            _cameraServiceMock.Setup(x => x.GetAllCamerasAsync())
                .ReturnsAsync(cameras);

            var cameraId = "1";
            var start = DateTime.UtcNow.AddDays(-1);
            var end = DateTime.UtcNow;

            // Act
            var result = await _controller.Index(cameraId, start, end) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cameras, result.ViewBag.Cameras);
            Assert.Equal(cameraId, result.ViewBag.SelectedCameraId);
            Assert.Equal(start, result.ViewBag.StartDate);
            Assert.Equal(end, result.ViewBag.EndDate);
        }

        [Fact]
        public async Task GetImages_ReturnsCorrectJsonResult()
        {
            // Arrange
            var events = new List<MotionEvent>
            {
                new MotionEvent { Id = 1, CameraId = "1", CameraName = "Camera 1" },
                new MotionEvent { Id = 2, CameraId = "1", CameraName = "Camera 1" }
            };

            _analyticsServiceMock.Setup(x => x.GetMotionEventsAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(events);

            _analyticsServiceMock.Setup(x => x.GetMotionEventCountAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .ReturnsAsync(2);

            // Act
            var result = await _controller.GetImages() as JsonResult;

            // Assert
            Assert.NotNull(result);
            dynamic data = result.Value;
            Assert.Equal(2, data.totalEvents);
            Assert.Equal(1, data.currentPage);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((MotionEvent)null);

            // Act
            var result = await _controller.Details(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewWithEventAndRelatedEvents()
        {
            // Arrange
            var motionEvent = new MotionEvent
            {
                Id = 1,
                CameraId = "1",
                CameraName = "Camera 1",
                Timestamp = DateTime.UtcNow
            };

            var relatedEvents = new List<MotionEvent>
            {
                new MotionEvent { Id = 2, CameraId = "1", Timestamp = DateTime.UtcNow.AddMinutes(1) }
            };

            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(1))
                .ReturnsAsync(motionEvent);

            _analyticsServiceMock.Setup(x => x.GetMotionEventsAsync(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .ReturnsAsync(relatedEvents);

            // Act
            var result = await _controller.Details(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(motionEvent, result.Model);
            Assert.NotNull(result.ViewBag.RelatedEvents);
        }

        [Fact]
        public async Task DeleteImage_ReturnsSuccess_WhenDeletionSucceeds()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.DeleteEventAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteImage(1) as JsonResult;

            // Assert
            Assert.NotNull(result);
            dynamic value = result.Value;
            Assert.True((bool)value.success);
        }

        [Fact]
        public async Task DeleteImage_ReturnsFailure_WhenDeletionFails()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.DeleteEventAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.DeleteImage(1) as JsonResult;

            // Assert
            Assert.NotNull(result);
            dynamic value = result.Value;
            Assert.False((bool)value.success);
        }

        [Fact]
        public async Task BulkDelete_ReturnsSuccess_WhenAllDeletionsSucceed()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.DeleteEventAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.BulkDelete(new[] { 1, 2, 3 }) as JsonResult;

            // Assert
            Assert.NotNull(result);
            dynamic value = result.Value;
            Assert.True((bool)value.success);
        }

        [Fact]
        public async Task Download_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((MotionEvent)null);

            // Act
            var result = await _controller.Download(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Download_ReturnsFile_WhenEventExists()
        {
            // Arrange
            var motionEvent = new MotionEvent
            {
                Id = 1,
                CameraName = "Camera 1",
                ImagePath = "C:/images/test.jpg",
                Timestamp = DateTime.UtcNow
            };

            _analyticsServiceMock.Setup(x => x.GetEventByIdAsync(1))
                .ReturnsAsync(motionEvent);

            // Act
            var result = await _controller.Download(1) as PhysicalFileResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("image/jpeg", result.ContentType);
            Assert.Contains(motionEvent.CameraName, result.FileDownloadName);
        }
    }
}
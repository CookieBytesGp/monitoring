 using Xunit;
using Moq;
using App.Services;
using App.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using App.Models;

namespace App.Tests.Services
{
    public class MotionDetectionServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILoggingService> _loggingServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ICameraService> _cameraServiceMock;
        private readonly MotionDetectionService _service;

        public MotionDetectionServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _loggingServiceMock = new Mock<ILoggingService>();
            _emailServiceMock = new Mock<IEmailService>();
            _cameraServiceMock = new Mock<ICameraService>();

            // Setup configuration
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(s => s.Value).Returns("0.3");
            _configMock.Setup(c => c.GetSection("MotionDetection:DefaultSensitivity"))
                      .Returns(configSection.Object);

            _service = new MotionDetectionService(
                _configMock.Object,
                _loggingServiceMock.Object,
                _emailServiceMock.Object,
                _cameraServiceMock.Object
            );
        }

        [Fact]
        public async Task GetSettingsAsync_ReturnsDefaultSettings_WhenNotPreviouslySet()
        {
            // Arrange
            string cameraId = "test-camera";

            // Act
            var settings = await _service.GetSettingsAsync(cameraId);

            // Assert
            Assert.NotNull(settings);
            Assert.True(settings.IsActive);
            Assert.Equal(0.3, settings.Sensitivity);
            Assert.NotNull(settings.RegionOfInterest);
            Assert.Equal(0, settings.RegionOfInterest.X);
            Assert.Equal(0, settings.RegionOfInterest.Y);
            Assert.Equal(0, settings.RegionOfInterest.Width);
            Assert.Equal(0, settings.RegionOfInterest.Height);
        }

        [Fact]
        public async Task UpdateSensitivityAsync_ClampsSensitivityValue()
        {
            // Arrange
            string cameraId = "test-camera";
            
            // Act & Assert - Test lower bound
            await _service.UpdateSensitivityAsync(cameraId, -0.5);
            var settings = await _service.GetSettingsAsync(cameraId);
            Assert.Equal(0.0, settings.Sensitivity);

            // Act & Assert - Test upper bound
            await _service.UpdateSensitivityAsync(cameraId, 1.5);
            settings = await _service.GetSettingsAsync(cameraId);
            Assert.Equal(1.0, settings.Sensitivity);

            // Act & Assert - Test valid value
            await _service.UpdateSensitivityAsync(cameraId, 0.7);
            settings = await _service.GetSettingsAsync(cameraId);
            Assert.Equal(0.7, settings.Sensitivity);
        }

        [Fact]
        public async Task StartDetectionAsync_LogsStartEvent()
        {
            // Arrange
            string cameraId = "test-camera";
            _cameraServiceMock.Setup(s => s.GetCameraByIdAsync(cameraId))
                            .ReturnsAsync(new CameraDevice { Id = cameraId, Name = "Test Camera" });

            // Act
            await _service.StartDetectionAsync(cameraId);

            // Assert
            _loggingServiceMock.Verify(
                x => x.LogSystemEventAsync(
                    "MotionDetection",
                    It.Is<string>(msg => msg.Contains(cameraId)),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task StopDetectionAsync_LogsStopEvent()
        {
            // Arrange
            string cameraId = "test-camera";
            await _service.StartDetectionAsync(cameraId);

            // Act
            await _service.StopDetectionAsync(cameraId);

            // Assert
            _loggingServiceMock.Verify(
                x => x.LogSystemEventAsync(
                    "MotionDetection",
                    It.Is<string>(msg => msg.Contains(cameraId)),
                    It.IsAny<string>()
                ),
                Times.Exactly(2) // Once for start, once for stop
            );
        }

        [Fact]
        public async Task IsDetectionActiveAsync_ReturnsCorrectState()
        {
            // Arrange
            string cameraId = "test-camera";
            
            // Act & Assert - Initially inactive
            var isActive = await _service.IsDetectionActiveAsync(cameraId);
            Assert.False(isActive);

            // Act & Assert - After starting
            await _service.StartDetectionAsync(cameraId);
            isActive = await _service.IsDetectionActiveAsync(cameraId);
            Assert.True(isActive);

            // Act & Assert - After stopping
            await _service.StopDetectionAsync(cameraId);
            isActive = await _service.IsDetectionActiveAsync(cameraId);
            Assert.False(isActive);
        }

        [Fact]
        public async Task UpdateRegionOfInterestAsync_UpdatesSettings()
        {
            // Arrange
            string cameraId = "test-camera";
            var roi = new Rectangle
            {
                X = 100,
                Y = 100,
                Width = 500,
                Height = 300
            };

            // Act
            await _service.UpdateRegionOfInterestAsync(cameraId, roi);
            var settings = await _service.GetSettingsAsync(cameraId);

            // Assert
            Assert.Equal(roi.X, settings.RegionOfInterest.X);
            Assert.Equal(roi.Y, settings.RegionOfInterest.Y);
            Assert.Equal(roi.Width, settings.RegionOfInterest.Width);
            Assert.Equal(roi.Height, settings.RegionOfInterest.Height);
        }

        [Fact]
        public void Dispose_CancelsAllDetectionTasks()
        {
            // Arrange
            string[] cameraIds = { "camera1", "camera2", "camera3" };
            foreach (var cameraId in cameraIds)
            {
                _service.StartDetectionAsync(cameraId).Wait();
            }

            // Act
            _service.Dispose();

            // Assert
            foreach (var cameraId in cameraIds)
            {
                var isActive = _service.IsDetectionActiveAsync(cameraId).Result;
                Assert.False(isActive);
            }
        }
    }
}
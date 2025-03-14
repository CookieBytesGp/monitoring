 using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Data;
using App.Models;
using App.Services;
using App.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace App.Tests.Services
{
    public class MotionAnalyticsServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILoggingService> _loggingServiceMock;
        private readonly MotionAnalyticsService _service;

        public MotionAnalyticsServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _loggingServiceMock = new Mock<ILoggingService>();
            _service = new MotionAnalyticsService(_context, _loggingServiceMock.Object);
        }

        [Fact]
        public async Task GetAnalyticsAsync_ReturnsCorrectData()
        {
            // Arrange
            var cameraId = "camera1";
            var start = DateTime.UtcNow.AddDays(-1);
            var end = DateTime.UtcNow;

            await SeedTestEvents(cameraId, start, end);

            // Act
            var result = await _service.GetAnalyticsAsync(cameraId, start, end);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalEvents > 0);
            Assert.True(result.AverageMotionPercentage >= 0);
            Assert.True(result.PeakMotionPercentage >= 0);
            Assert.NotNull(result.EventsByHour);
        }

        [Fact]
        public async Task GetEventsByHourAsync_ReturnsCorrectDistribution()
        {
            // Arrange
            var cameraId = "camera1";
            var date = DateTime.UtcNow.Date;
            await SeedTestEvents(cameraId, date, date.AddDays(1));

            // Act
            var result = await _service.GetEventsByHourAsync(cameraId, date);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            Assert.All(result.Keys, hour => Assert.InRange(hour, 0, 23));
        }

        [Fact]
        public async Task GetRecentEventsAsync_ReturnsCorrectNumberOfEvents()
        {
            // Arrange
            const int count = 5;
            await SeedTestEvents("camera1", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);

            // Act
            var result = await _service.GetRecentEventsAsync(count);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count <= count);
        }

        [Fact]
        public async Task AcknowledgeEventAsync_UpdatesEventCorrectly()
        {
            // Arrange
            var eventId = await SeedSingleEvent();
            var username = "testuser";

            // Act
            await _service.AcknowledgeEventAsync(eventId, username);

            // Assert
            var updatedEvent = await _context.MotionEvents.FindAsync(eventId);
            Assert.True(updatedEvent.Acknowledged);
            Assert.Equal(username, updatedEvent.AcknowledgedBy);
            Assert.NotNull(updatedEvent.AcknowledgedAt);
        }

        [Fact]
        public async Task GetEventCountByCamera_ReturnsCorrectCounts()
        {
            // Arrange
            var start = DateTime.UtcNow.AddDays(-1);
            var end = DateTime.UtcNow;
            await SeedTestEvents("camera1", start, end);
            await SeedTestEvents("camera2", start, end);

            // Act
            var result = await _service.GetEventCountByCamera(start, end);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            Assert.Contains("camera1", result.Keys);
            Assert.Contains("camera2", result.Keys);
        }

        private async Task<int> SeedSingleEvent()
        {
            var motionEvent = new MotionEvent
            {
                CameraId = "camera1",
                CameraName = "Test Camera",
                Timestamp = DateTime.UtcNow,
                MotionPercentage = 0.5f,
                Location = "Test Location"
            };

            _context.MotionEvents.Add(motionEvent);
            await _context.SaveChangesAsync();
            return motionEvent.Id;
        }

        private async Task SeedTestEvents(string cameraId, DateTime start, DateTime end)
        {
            var events = new List<MotionEvent>();
            var random = new Random();

            for (var i = 0; i < 10; i++)
            {
                events.Add(new MotionEvent
                {
                    CameraId = cameraId,
                    CameraName = $"Camera {cameraId}",
                    Timestamp = start.AddHours(random.Next(0, (int)(end - start).TotalHours)),
                    MotionPercentage = (float)random.NextDouble(),
                    Location = "Test Location"
                });
            }

            await _context.MotionEvents.AddRangeAsync(events);
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
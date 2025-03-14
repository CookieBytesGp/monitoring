 using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using App.Data;
using App.Models;
using App.Services.BackgroundServices;
using App.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace App.Tests.Services
{
    public class ReportSchedulerServiceTests
    {
        private readonly Mock<ILogger<ReportSchedulerService>> _loggerMock;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IReportGenerationService> _reportServiceMock;
        private readonly Mock<ApplicationDbContext> _contextMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;

        public ReportSchedulerServiceTests()
        {
            _loggerMock = new Mock<ILogger<ReportSchedulerService>>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _reportServiceMock = new Mock<IReportGenerationService>();
            _contextMock = new Mock<ApplicationDbContext>();
            _scopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _scopeFactoryMock.Setup(x => x.CreateScope())
                .Returns(_scopeMock.Object);
            _scopeMock.Setup(x => x.ServiceProvider)
                .Returns(_serviceProviderMock.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext)))
                .Returns(_contextMock.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(IReportGenerationService)))
                .Returns(_reportServiceMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ProcessesScheduledReports()
        {
            // Arrange
            var templates = new List<ReportTemplate>
            {
                new ReportTemplate
                {
                    Id = 1,
                    Name = "Daily Report",
                    IsScheduled = true,
                    Schedule = "0 0 * * *", // Daily at midnight
                    Format = "CSV",
                    TimeRangeType = "Daily",
                    EmailRecipients = "test@example.com"
                }
            };

            var dbSet = MockDbSet(templates);
            _contextMock.Setup(x => x.ReportTemplates).Returns(dbSet.Object);

            _reportServiceMock.Setup(x => x.GenerateCsvReportAsync(
                It.IsAny<ReportTemplate>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });

            _reportServiceMock.Setup(x => x.SendReportEmailAsync(
                It.IsAny<ReportTemplate>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>()))
                .ReturnsAsync(true);

            var service = new ReportSchedulerService(_loggerMock.Object, _scopeFactoryMock.Object);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var task = service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(2000); // Wait for the service to process
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            _reportServiceMock.Verify(x => x.GenerateCsvReportAsync(
                It.IsAny<ReportTemplate>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProcessScheduledReports_HandlesInvalidCronExpression()
        {
            // Arrange
            var templates = new List<ReportTemplate>
            {
                new ReportTemplate
                {
                    Id = 1,
                    Name = "Invalid Schedule",
                    IsScheduled = true,
                    Schedule = "invalid cron",
                    Format = "CSV"
                }
            };

            var dbSet = MockDbSet(templates);
            _contextMock.Setup(x => x.ReportTemplates).Returns(dbSet.Object);

            var service = new ReportSchedulerService(_loggerMock.Object, _scopeFactoryMock.Object);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var task = service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(2000);
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            _reportServiceMock.Verify(x => x.GenerateCsvReportAsync(
                It.IsAny<ReportTemplate>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public async Task ProcessScheduledReports_HandlesEmailFailure()
        {
            // Arrange
            var templates = new List<ReportTemplate>
            {
                new ReportTemplate
                {
                    Id = 1,
                    Name = "Email Failure Test",
                    IsScheduled = true,
                    Schedule = "0 0 * * *",
                    Format = "CSV",
                    EmailRecipients = "test@example.com"
                }
            };

            var dbSet = MockDbSet(templates);
            _contextMock.Setup(x => x.ReportTemplates).Returns(dbSet.Object);

            _reportServiceMock.Setup(x => x.GenerateCsvReportAsync(
                It.IsAny<ReportTemplate>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });

            _reportServiceMock.Setup(x => x.SendReportEmailAsync(
                It.IsAny<ReportTemplate>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>()))
                .ReturnsAsync(false);

            var service = new ReportSchedulerService(_loggerMock.Object, _scopeFactoryMock.Object);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var task = service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(2000);
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send report email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Theory]
        [InlineData("Daily", -1)]
        [InlineData("Weekly", -7)]
        [InlineData("Monthly", -30)]
        [InlineData("Custom", -14)]
        public async Task GenerateAndSendReport_CalculatesCorrectDateRange(string timeRangeType, int expectedDays)
        {
            // Arrange
            var template = new ReportTemplate
            {
                Id = 1,
                Name = "Date Range Test",
                IsScheduled = true,
                Schedule = "0 0 * * *",
                Format = "CSV",
                TimeRangeType = timeRangeType,
                CustomDays = timeRangeType == "Custom" ? 14 : null
            };

            DateTime capturedStartDate = DateTime.MinValue;
            DateTime capturedEndDate = DateTime.MinValue;

            _reportServiceMock.Setup(x => x.GenerateCsvReportAsync(
                It.IsAny<ReportTemplate>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
                .Callback<ReportTemplate, DateTime, DateTime>((t, start, end) =>
                {
                    capturedStartDate = start;
                    capturedEndDate = end;
                })
                .ReturnsAsync(new byte[] { 1, 2, 3 });

            var dbSet = MockDbSet(new List<ReportTemplate> { template });
            _contextMock.Setup(x => x.ReportTemplates).Returns(dbSet.Object);

            var service = new ReportSchedulerService(_loggerMock.Object, _scopeFactoryMock.Object);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var task = service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(2000);
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            Assert.True((capturedEndDate - capturedStartDate).Days == Math.Abs(expectedDays));
        }

        private static Mock<DbSet<T>> MockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            return dbSet;
        }
    }
}
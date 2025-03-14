 using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Cronos;
using App.Data;
using App.Models;
using App.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace App.Services.BackgroundServices
{
    public class ReportSchedulerService : BackgroundService
    {
        private readonly ILogger<ReportSchedulerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ReportSchedulerService(
            ILogger<ReportSchedulerService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Report Scheduler Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessScheduledReports(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing scheduled reports");
                }

                // Wait for 1 minute before checking again
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ProcessScheduledReports(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var reportService = scope.ServiceProvider.GetRequiredService<IReportGenerationService>();

            var scheduledTemplates = await context.ReportTemplates
                .Where(t => t.IsScheduled && !string.IsNullOrEmpty(t.Schedule))
                .ToListAsync(stoppingToken);

            foreach (var template in scheduledTemplates)
            {
                try
                {
                    if (await ShouldGenerateReport(template))
                    {
                        await GenerateAndSendReport(template, reportService);
                        
                        // Update last generation time
                        template.LastGeneratedAt = DateTime.UtcNow;
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing scheduled report for template {TemplateId}", template.Id);
                }
            }
        }

        private async Task<bool> ShouldGenerateReport(ReportTemplate template)
        {
            try
            {
                if (string.IsNullOrEmpty(template.Schedule))
                    return false;

                var cronExpression = CronExpression.Parse(template.Schedule);
                var lastRun = template.LastGeneratedAt ?? DateTime.MinValue;
                var nextRun = cronExpression.GetNextOccurrence(lastRun, TimeZoneInfo.Utc);

                return nextRun <= DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing cron expression for template {TemplateId}", template.Id);
                return false;
            }
        }

        private async Task GenerateAndSendReport(ReportTemplate template, IReportGenerationService reportService)
        {
            try
            {
                // Calculate date range based on template settings
                var endDate = DateTime.UtcNow;
                var startDate = template.TimeRangeType switch
                {
                    "Daily" => endDate.AddDays(-1),
                    "Weekly" => endDate.AddDays(-7),
                    "Monthly" => endDate.AddMonths(-1),
                    "Custom" => endDate.AddDays(-(template.CustomDays ?? 7)),
                    _ => endDate.AddDays(-7)
                };

                // Generate report based on format
                byte[] reportData;
                switch (template.Format.ToUpper())
                {
                    case "CSV":
                        reportData = await reportService.GenerateCsvReportAsync(template, startDate, endDate);
                        break;
                    case "JSON":
                        reportData = await reportService.GenerateJsonReportAsync(template, startDate, endDate);
                        break;
                    case "EXCEL":
                        reportData = await reportService.GenerateExcelReportAsync(template, startDate, endDate);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported format: {template.Format}");
                }

                // Send report via email
                var emailSent = await reportService.SendReportEmailAsync(template, reportData, template.Format);
                if (!emailSent)
                {
                    throw new Exception("Failed to send report email");
                }

                _logger.LogInformation(
                    "Successfully generated and sent scheduled report for template {TemplateId}, Format: {Format}",
                    template.Id,
                    template.Format
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to generate and send scheduled report for template {TemplateId}",
                    template.Id
                );
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Report Scheduler Service is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}
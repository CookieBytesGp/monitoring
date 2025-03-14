using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using App.Data;
using App.Models;
using App.Services.Interfaces; 
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Services
{
    public class ReportGenerationService : IReportGenerationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMotionAnalyticsService _analyticsService;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReportGenerationService> _logger;

        public ReportGenerationService(
            ApplicationDbContext context,
            IMotionAnalyticsService analyticsService,
            IImageProcessingService imageProcessingService,
            IEmailService emailService,
            ILogger<ReportGenerationService> logger)
        {
            _context = context;
            _analyticsService = analyticsService;
            _imageProcessingService = imageProcessingService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<byte[]> GenerateCsvReportAsync(ReportTemplate template, DateTime startDate, DateTime endDate)
        {
            try
            {
                var data = await GetReportDataAsync(template, startDate, endDate);
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream, Encoding.UTF8);

                // Write headers
                var headers = GetHeaders(template);
                await writer.WriteLineAsync(string.Join(",", headers));

                // Write data rows
                foreach (var item in data)
                {
                    var row = FormatDataRow(item, template);
                    await writer.WriteLineAsync(string.Join(",", row.Select((Func<dynamic, string>)(field => EscapeCsvField((string)Convert.ChangeType(field, typeof(string)))))));
                }

                await writer.FlushAsync();
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CSV report for template {TemplateId}", template.Id);
                throw;
            }
        }

        public async Task<byte[]> GenerateJsonReportAsync(ReportTemplate template, DateTime startDate, DateTime endDate)
        {
            try
            {
                var data = await GetReportDataAsync(template, startDate, endDate);
                var jsonData = new
                {
                    ReportInfo = new
                    {
                        TemplateName = template.Name,
                        GeneratedAt = DateTime.UtcNow,
                        TimeRange = new { StartDate = startDate, EndDate = endDate }
                    },
                    Data = data
                };

                return JsonSerializer.SerializeToUtf8Bytes(jsonData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JSON report for template {TemplateId}", template.Id);
                throw;
            }
        }

        public async Task<byte[]> GenerateExcelReportAsync(ReportTemplate template, DateTime startDate, DateTime endDate)
        {
            try
            {
                var data = await GetReportDataAsync(template, startDate, endDate);
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Report Data");

                // Add headers
                var headers = GetHeaders(template);
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                }

                // Add data
                int row = 2;
                foreach (var item in data)
                {
                    var rowData = FormatDataRow(item, template);
                    for (int i = 0; i < rowData.Count; i++)
                    {
                        worksheet.Cell(row, i + 1).Value = rowData[i];
                    }
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using var memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report for template {TemplateId}", template.Id);
                throw;
            }
        }

        public Task<string> GetContentTypeForFormat(string format)
        {
            return Task.FromResult(format.ToUpper() switch
            {
                "CSV" => "text/csv",
                "JSON" => "application/json",
                "EXCEL" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new ArgumentException("Unsupported format", nameof(format))
            });
        }

        public Task<string> GetFileExtensionForFormat(string format)
        {
            return Task.FromResult(format.ToUpper() switch
            {
                "CSV" => ".csv",
                "JSON" => ".json",
                "EXCEL" => ".xlsx",
                _ => throw new ArgumentException("Unsupported format", nameof(format))
            });
        }

        public async Task<bool> SendReportEmailAsync(ReportTemplate template, byte[] reportData, string format)
        {
            try
            {
                // Validate input parameters
                if (template == null)
                    throw new ArgumentNullException(nameof(template));
                if (reportData == null || reportData.Length == 0)
                    throw new ArgumentException("Report data cannot be empty", nameof(reportData));
                if (string.IsNullOrEmpty(format))
                    throw new ArgumentException("Format cannot be empty", nameof(format));
                if (string.IsNullOrEmpty(template.EmailRecipients))
                    return false;

                // Validate email addresses
                var emailAddresses = template.EmailRecipients
                    .Split(',')
                    .Select(e => e.Trim())
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList();

                if (!emailAddresses.Any())
                {
                    _logger.LogWarning("No valid email addresses found in template {TemplateId}", template.Id);
                    return false;
                }

                // Check attachment size (e.g., 25MB limit)
                const int maxAttachmentSize = 25 * 1024 * 1024; // 25MB
                if (reportData.Length > maxAttachmentSize)
                {
                    _logger.LogError("Report file size ({Size} bytes) exceeds maximum allowed size ({MaxSize} bytes) for template {TemplateId}", 
                        reportData.Length, maxAttachmentSize, template.Id);
                    return false;
                }

                // Get file information
                var extension = await GetFileExtensionForFormat(format);
                var fileName = $"Report_{template.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{extension}";
                var contentType = await GetContentTypeForFormat(format);

                // Create email model
                var emailModel = new EmailModel
                {
                    To = emailAddresses,
                    Subject = $"Motion Analytics Report: {template.Name}",
                    Body = $"""
                        Please find attached the report generated from template '{template.Name}'.
                        
                        Report Details:
                        - Template: {template.Name}
                        - Format: {format.ToUpper()}
                        - Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
                        
                        This is an automated message. Please do not reply.
                        """,
                    Attachments = new List<EmailAttachment>
                    {
                        new EmailAttachment
                        {
                            FileName = fileName,
                            Content = reportData,
                            ContentType = contentType
                        }
                    }
                };

                // Send email
                await _emailService.SendEmailAsync(emailModel.Subject, emailModel.Body, emailModel.To, emailModel.Attachments);
                
                _logger.LogInformation(
                    "Report email sent successfully for template {TemplateId} to {RecipientCount} recipients", 
                    template.Id, 
                    emailAddresses.Count);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error sending report email for template {TemplateId}. Error: {ErrorMessage}", 
                    template?.Id ?? 0, 
                    ex.Message);
                return false;
            }
        }

        private async Task<List<dynamic>> GetReportDataAsync(ReportTemplate template, DateTime startDate, DateTime endDate)
        {
            var query = _context.MotionEvents.AsQueryable();

            // Apply template filters
            if (!string.IsNullOrEmpty(template.CameraFilter))
            {
                var cameraIds = template.CameraFilter
                    .Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => int.TryParse(s.Trim(), out int id) ? id : -1)
                    .Where(id => id != -1)
                    .ToList();

                if (cameraIds.Any())
                {
                    query = query.Where(e => cameraIds.Contains(e.CameraId));
                }
            }

            if (template.MinMotionPercentage.HasValue)
            {
                query = query.Where(e => (float)e.MotionPercentage >= template.MinMotionPercentage.Value);
            }

            if (template.MaxMotionPercentage.HasValue)
            {
                query = query.Where(e => (float)e.MotionPercentage <= template.MaxMotionPercentage.Value);
            }

            if (template.OnlyAcknowledged)
            {
                query = query.Where(e => e.Acknowledged);
            }

            // Apply date range
            query = query.Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate);

            var events = await query
                .Include(e => e.Camera)
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();

            var result = new List<dynamic>();

            foreach (var evt in events)
            {
                var dataItem = new Dictionary<string, object>
                {
                    ["EventId"] = evt.Id,
                    ["Timestamp"] = evt.Timestamp,
                    ["CameraName"] = evt.Camera.Name,
                    ["MotionPercentage"] = evt.MotionPercentage,
                    ["IsAcknowledged"] = evt.Acknowledged
                };

                if (template.IncludeMotionStats)
                {
                    var stats = await _analyticsService.GetMotionStatsAsync(evt.Id);
                    dataItem["MotionStats"] = stats;
                }

                if (template.IncludeImageAnalysis)
                {
                    var analysis = await _imageProcessingService.AnalyzeImageQualityAsync(evt.ImagePath);
                    dataItem["ImageAnalysis"] = analysis;
                }

                if (template.IncludeProcessingHistory)
                {
                    var history = await _context.ImageProcessingHistory
                        .Where(h => h.MotionEventId == evt.Id)
                        .OrderByDescending(h => h.ProcessedAt)
                        .ToListAsync();
                    dataItem["ProcessingHistory"] = history;
                }

                result.Add(dataItem);
            }

            return result;
        }

        private List<string> GetHeaders(ReportTemplate template)
        {
            var headers = new List<string>
            {
                "Event ID",
                "Timestamp",
                "Camera Name",
                "Motion Percentage",
                "Acknowledged"
            };

            if (template.IncludeMotionStats)
            {
                headers.AddRange(new[] { "Motion Area", "Motion Duration", "Peak Motion Time" });
            }

            if (template.IncludeImageAnalysis)
            {
                headers.AddRange(new[] { "Image Quality", "Blur Level", "Brightness" });
            }

            if (template.IncludeProcessingHistory)
            {
                headers.AddRange(new[] { "Processing Type", "Processed At", "Success" });
            }

            return headers;
        }

        private List<string> FormatDataRow(dynamic item, ReportTemplate template)
        {
            var row = new List<string>
            {
                item.EventId.ToString(),
                item.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                item.CameraName,
                item.MotionPercentage.ToString("F2"),
                item.IsAcknowledged ? "Yes" : "No"
            };

            if (template.IncludeMotionStats)
            {
                var stats = item.MotionStats;
                row.AddRange(new string[]
                {
                    stats.MotionArea.ToString("F2"),
                    stats.Duration.ToString(),
                    stats.PeakTime.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            if (template.IncludeImageAnalysis)
            {
                var analysis = item.ImageAnalysis;
                row.AddRange(new string[]
                {
                    analysis.Quality.ToString("F2"),
                    analysis.BlurLevel.ToString("F2"),
                    analysis.Brightness.ToString("F2")
                });
            }

            if (template.IncludeProcessingHistory)
            {
                var history = ((IEnumerable<dynamic>)item.ProcessingHistory).FirstOrDefault();
                if (history != null)
                {
                    row.AddRange(new string[]
                    {
                        history.ProcessingType,
                        history.ProcessedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        history.Success ? "Yes" : "No"
                    });
                }
                else
                {
                    row.AddRange(new[] { "", "", "" });
                }
            }

            return row;
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }
    }
}
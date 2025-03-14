using System;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Services.Interfaces;
using CsvHelper;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class ExportController : Controller
    {
        private readonly IMotionAnalyticsService _analyticsService;
        private readonly ICameraService _cameraService;
        private readonly ILoggingService _loggingService;

        public ExportController(
            IMotionAnalyticsService analyticsService,
            ICameraService cameraService,
            ILoggingService loggingService)
        {
            _analyticsService = analyticsService;
            _cameraService = cameraService;
            _loggingService = loggingService;
        }

        [HttpGet]
        public async Task<IActionResult> ExportEvents(
            string cameraId = null,
            DateTime? start = null,
            DateTime? end = null,
            string format = "csv")
        {
            try
            {
                var startDate = start ?? DateTime.UtcNow.AddDays(-7);
                var endDate = end ?? DateTime.UtcNow;
                
                var events = await _analyticsService.GetMotionEventsAsync(
                    startDate,
                    endDate,
                    cameraId);

                var fileName = $"motion_events_{DateTime.UtcNow:yyyyMMdd_HHmmss}";

                switch (format.ToLower())
                {
                    case "csv":
                        return ExportToCsv(events, fileName);
                    case "json":
                        return ExportToJson(events, fileName);
                    case "excel":
                        return ExportToExcel(events, fileName);
                    default:
                        return BadRequest("Unsupported format");
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex , "Failed to export events", ex.Message);
                return StatusCode(500, "Failed to export data");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportAnalytics(
            string cameraId = null,
            DateTime? start = null,
            DateTime? end = null,
            string format = "csv")
        {
            try
            {
                var analytics = await _analyticsService.GetAnalyticsAsync(
                    cameraId,
                    start ?? DateTime.UtcNow.AddDays(-7),
                    end ?? DateTime.UtcNow);

                var fileName = $"motion_analytics_{DateTime.UtcNow:yyyyMMdd_HHmmss}";

                switch (format.ToLower())
                {
                    case "csv":
                        return ExportToCsv(new[] { analytics }, fileName);
                    case "json":
                        return ExportToJson(analytics, fileName);
                    case "excel":
                        return ExportToExcel(new[] { analytics }, fileName);
                    default:
                        return BadRequest("Unsupported format");
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(ex , "Failed to export analytics", ex.Message);
                return StatusCode(500, "Failed to export data");
            }
        }

        private FileResult ExportToCsv<T>(IEnumerable<T> data, string fileName)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(data);
            writer.Flush();

            return File(
                memoryStream.ToArray(),
                "text/csv",
                $"{fileName}.csv");
        }

        private FileResult ExportToJson<T>(T data, string fileName)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            var bytes = Encoding.UTF8.GetBytes(json);

            return File(
                bytes,
                "application/json",
                $"{fileName}.json");
        }

        private FileResult ExportToExcel<T>(IEnumerable<T> data, string fileName)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Data");

            // Get properties
            var properties = typeof(T).GetProperties();

            // Write headers
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = properties[i].Name;
            }

            // Write data
            var row = 2;
            foreach (var item in data)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cell(row, i + 1).Value = 
                        properties[i].GetValue(item)?.ToString() ?? "";
                }
                row++;
            }

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);

            return File(
                memoryStream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{fileName}.xlsx");
        }
    }
}
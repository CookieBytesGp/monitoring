 using System.Threading.Tasks;
using App.Models;

namespace App.Services.Interfaces
{
    public interface IReportGenerationService
    {
        Task<byte[]> GenerateCsvReportAsync(ReportTemplate template, DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateJsonReportAsync(ReportTemplate template, DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateExcelReportAsync(ReportTemplate template, DateTime startDate, DateTime endDate);
        Task<string> GetContentTypeForFormat(string format);
        Task<string> GetFileExtensionForFormat(string format);
        Task<bool> SendReportEmailAsync(ReportTemplate template, byte[] reportData, string format);
    }
}
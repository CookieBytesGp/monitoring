using System.Collections.Generic;
using System.Threading.Tasks;
using App.Models;

namespace App.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string subject, string body, List<string> to, List<EmailAttachment> attachments, bool isHtml = false);
        Task SendDeviceAlertAsync(string deviceType, string deviceName, string status, string details);
        Task SendMotionDetectionAlertAsync(string cameraName, string location, DateTime timestamp);
        Task SendSystemAlertAsync(string title, string message, string severity);
    }
}
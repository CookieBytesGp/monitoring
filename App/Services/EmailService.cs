using App.Services.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using App.Models;

namespace App.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly bool _emailEnabled;
        private readonly string _notificationEmail;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _smtpServer = configuration["Smtp:Server"];
            _smtpPort = configuration["Smtp:Port"] != null ? 
                int.Parse(configuration["Smtp:Port"]) : 
                587; // Default SMTP port if not specified
            _smtpUsername = configuration["Smtp:Username"];
            _smtpPassword = configuration["Smtp:Password"];
            _senderEmail = configuration["Smtp:SenderEmail"];
            _senderName = configuration["Smtp:SenderName"];
            _emailEnabled = bool.Parse(configuration["Smtp:Enabled"]);
            _notificationEmail = configuration.GetValue<string>("Settings:NotificationEmail");
        }

        public async Task SendEmailAsync(string subject, string body, List<string> to, List<EmailAttachment> attachments, bool isHtml)
        {
            if (!_emailEnabled)
            {
                _logger.LogInformation("Email notifications are disabled");
                return;
            }

            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_senderEmail, _senderName);
                foreach (var recipient in to)
                {
                    message.To.Add(new MailAddress(recipient));
                }
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                foreach (var attachment in attachments)
                {
                    message.Attachments.Add(new Attachment(new MemoryStream(attachment.Content), attachment.FileName, attachment.ContentType));
                }

                using var client = new SmtpClient(_smtpServer, _smtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {string.Join(", ", to)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {string.Join(", ", to)}");
                throw;
            }
        }

        public async Task SendDeviceAlertAsync(string deviceType, string deviceName, string status, string details)
        {
            if (string.IsNullOrEmpty(_notificationEmail)) return;

            var subject = $"{deviceType} Alert: {deviceName} - {status}";
            var body = $@"
                <h2>{deviceType} Status Alert</h2>
                <p><strong>Device:</strong> {deviceName}</p>
                <p><strong>Status:</strong> {status}</p>
                <p><strong>Details:</strong> {details}</p>
                <p><strong>Time:</strong> {DateTime.UtcNow:g} UTC</p>
                <hr>
                <p>This is an automated message from your Monitoring System.</p>";

            await SendEmailAsync(subject, body, new List<string> { _notificationEmail }, new List<EmailAttachment>(), true);
        }

        public async Task SendMotionDetectionAlertAsync(string cameraName, string location, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(_notificationEmail)) return;

            var subject = $"Motion Detected: {cameraName}";
            var body = $@"
                <h2>Motion Detection Alert</h2>
                <p><strong>Camera:</strong> {cameraName}</p>
                <p><strong>Location:</strong> {location}</p>
                <p><strong>Time:</strong> {timestamp:g} UTC</p>
                <hr>
                <p>Motion has been detected by your monitoring system.</p>
                <p>Please check your camera feed for more details.</p>";

            await SendEmailAsync(subject, body, new List<string> { _notificationEmail }, new List<EmailAttachment>(), true);
        }

        public async Task SendSystemAlertAsync(string title, string message, string severity)
        {
            if (string.IsNullOrEmpty(_notificationEmail)) return;

            var subject = $"System Alert: {title}";
            var body = $@"
                <h2>System Alert</h2>
                <p><strong>Title:</strong> {title}</p>
                <p><strong>Severity:</strong> {severity}</p>
                <p><strong>Message:</strong> {message}</p>
                <p><strong>Time:</strong> {DateTime.UtcNow:g} UTC</p>
                <hr>
                <p>This is an automated message from your Monitoring System.</p>";

            await SendEmailAsync(subject, body, new List<string> { _notificationEmail }, new List<EmailAttachment>(), true);
        }
    }
}
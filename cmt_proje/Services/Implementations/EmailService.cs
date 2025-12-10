using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using cmt_proje.Services.Interfaces;

namespace cmt_proje.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            var host = _configuration["Email:Smtp:Host"];
            var port = _configuration.GetValue<int?>("Email:Smtp:Port") ?? 587;
            var enableSsl = _configuration.GetValue<bool?>("Email:Smtp:EnableSsl") ?? true;
            var fromEmail = _configuration["Email:Smtp:From"];
            var user = _configuration["Email:Smtp:User"];
            var password = _configuration["Email:Smtp:Password"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
            {
                _logger.LogWarning("Email settings are missing. Host or From address is empty. Email not sent.");
                return;
            }

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = string.IsNullOrWhiteSpace(user)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(user, password)
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}


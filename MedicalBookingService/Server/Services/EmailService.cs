using MedicalBookingService.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace MedicalBookingService.Server.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public EmailService(IConfiguration config, ILogger<AuthService> logger)
        { 
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(
                    _config["EmailSettings:Username"],
                    _config["EmailSettings:Password"]
                ),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:Username"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}

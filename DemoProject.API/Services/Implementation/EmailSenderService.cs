using DemoProject.API.Services.Interface;
using DemoProject.DataModels.Dto.System;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace DemoProject.API.Services.Implementation
{
    public class EmailSenderService : IEmailSender
    {
        private readonly EmailOptions _options;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(IOptions<EmailOptions> emailOptions, ILogger<EmailSenderService> logger)
        {
            _options = emailOptions.Value;
            _logger = logger;
        }

        public Task SendChangeEmailPassword(string toEmail, string url)
        => SendEmailAsync(toEmail, "Your password has been changed","");



        public Task SendForgotEmailAsync(string toEmail, string url)
            => SendEmailAsync(toEmail, "Password reset", $"<p>Click <a href='{url}'>here</a> to reset your password.</p>");

        public Task SendAccountConfirmationEmailAsync(string toEmail, string url)
         => SendEmailAsync(toEmail, "Confirm Email", $"<p>Click <a href='{url}'>here</a> to confirm your email.</p>");
        private async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var from = new MailAddress(_options.FromEmail, _options.FromName);
                var toAddr = new MailAddress(to);

                using var message = new MailMessage(from, toAddr)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                using var smtp = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
                {
                    EnableSsl = _options.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true
                };

                if (!string.IsNullOrWhiteSpace(_options.UserName))
                {
                    smtp.Credentials = new NetworkCredential(_options.UserName, _options.Password ?? string.Empty);
                }

                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", to);
                throw;
            }
        }

        
    }
}

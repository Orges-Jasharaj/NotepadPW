using DemoProject.API.Services.Interface;
using DemoProject.DataModels.Dto.System;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace DemoProject.API.Services.Implementation
{
    public class EmailSenderService : IEmailSender
    {
        private readonly MailerSendSettings _mailerSendSettings;
        private readonly ILogger<EmailSenderService> _logger;


        public EmailSenderService(IOptions<MailerSendSettings> mailerSendSettings, ILogger<EmailSenderService> logger)
        {
            _mailerSendSettings = mailerSendSettings.Value;
            _logger = logger;
        }

        public Task SendEmail(string to, string subject, string body)
        {
            try
            {
                var fromEmail = new MailAddress(_mailerSendSettings.Username);
                var toEmail = new MailAddress("orgesfx@gmail.com");

                var mailMessage = new MailMessage(fromEmail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                using (var smtpClient = new SmtpClient(_mailerSendSettings.Server, _mailerSendSettings.Port))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(_mailerSendSettings.Username, _mailerSendSettings.Password);
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(mailMessage);
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", to);
                return Task.FromException(ex);
            }
        }

        public async Task SendEmailWithTemplateAsync(string to, string subject, string firstName, string lastName)
        {
            try
            {
                var fromEmail = new MailAddress(_mailerSendSettings.Username);
                var toEmail = new MailAddress("orgesfx@gmail.com");

                var templatePath = Path.Combine(AppContext.BaseDirectory, "Template", "WelcomeEmail.html");

                var body = await File.ReadAllTextAsync(templatePath);

                body = body.Replace("[FirstName]", firstName)
           .Replace("[LastName]", lastName)
           .Replace("[AppName]", "E-Commerce Shop Api")
           .Replace("[Link]", "google.com");


                var mailMessage = new MailMessage(fromEmail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                using (var smtpClient = new SmtpClient(_mailerSendSettings.Server, _mailerSendSettings.Port))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(_mailerSendSettings.Username, _mailerSendSettings.Password);
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", to);
            }
        }
    }
}

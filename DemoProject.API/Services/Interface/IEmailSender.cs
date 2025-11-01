namespace DemoProject.API.Services.Interface
{
    public interface IEmailSender
    {
        Task SendEmail(string to, string subject, string body);
        Task SendEmailWithTemplateAsync(string to, string subject, string firstName, string lastName);
    }
}

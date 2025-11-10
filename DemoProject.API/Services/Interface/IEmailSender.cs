namespace DemoProject.API.Services.Interface
{
    public interface IEmailSender
    {
        Task SendForgotEmailAsync(string toEmail, string url);
        Task SendAccountConfirmationEmailAsync(string toEmail, string url);
        Task SendChangeEmailPassword(string toEmail, string url);
    }
}

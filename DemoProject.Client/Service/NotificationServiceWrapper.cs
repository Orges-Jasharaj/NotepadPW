using Radzen;

namespace DemoProject.Client.Service
{
    public class NotificationServiceWrapper(NotificationService notificationService)
    {
        private readonly NotificationService _notificationService = notificationService;

        public void ShowSuccess(string message, string title = "Success")
        {
            ShowNotification(message, title, NotificationSeverity.Success);
        }

        public void ShowError(string message, string title = "Error")
        {
            ShowNotification(message, title, NotificationSeverity.Error);
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            ShowNotification(message, title, NotificationSeverity.Warning);
        }

        public void ShowInfo(string message, string title = "Info")
        {
            ShowNotification(message, title, NotificationSeverity.Info);
        }

        private void ShowNotification(string message, string title, NotificationSeverity notificationSeverity)
        {
            _notificationService.Notify(new NotificationMessage
            {
                Severity = notificationSeverity,
                Summary = title,
                Detail = message,
                Duration = 30000,
            });
        }

    }
}

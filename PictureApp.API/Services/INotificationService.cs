using PictureApp.API.Services.NotificationTemplates;

namespace PictureApp.API.Services
{
    public interface INotificationService
    {
        void Send(string recipient, INotificationTemplateData notificationTemplate);
    }
}

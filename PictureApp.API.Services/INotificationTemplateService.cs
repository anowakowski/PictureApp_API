using System.Threading.Tasks;
using PictureApp.API.Dtos;
using PictureApp.Notifications;

namespace PictureApp.API.Services
{
    public interface INotificationTemplateService
    {
        Task<NotificationDto> CreateNotification(INotificationTemplateData templateData);
    }
}

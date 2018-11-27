using System.Threading.Tasks;
using PictureApp.API.Services.NotificationTemplateData;

namespace PictureApp.API.Services
{
    public interface INotificationService
    {
        Task SendAsync(string recipient, INotificationTemplateData templateData);
    }
}

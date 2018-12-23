using System.Threading.Tasks;
using PictureApp.Notifications;

namespace PictureApp.API.Services
{
    public interface INotificationService
    {
        Task SendAsync(string recipient, INotificationTemplateData templateData);
    }
}

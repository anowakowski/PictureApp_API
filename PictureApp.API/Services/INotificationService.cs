using System.Threading.Tasks;
using PictureApp.API.Services.NotificationTemplateData;

namespace PictureApp.API.Services
{
    public interface INotificationService
    {
        void SendAsync(string recipient, INotificationTemplateData templateData);
    }
}

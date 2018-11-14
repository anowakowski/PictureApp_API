using PictureApp.API.Data.Repository;
using PictureApp.API.Models;
using PictureApp.API.Services.NotificationTemplates;

namespace PictureApp.API.Services
{
    public class EmailNotificationService : INotificationService
    {
        private IRepository<NotificationTemplate> _repository;

        public void Send(string recipient, INotificationTemplateData notificationTemplate)
        {
            throw new System.NotImplementedException();
        }
    }
}

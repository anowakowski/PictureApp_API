using System;
using System.Text;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos;
using PictureApp.API.Models;
using PictureApp.Notifications;

namespace PictureApp.API.Services
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly IRepository<NotificationTemplate> _repository;

        public NotificationTemplateService(IRepository<NotificationTemplate> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<NotificationDto> CreateNotification(INotificationTemplateData templateData)
        {
            if (templateData == null)
            {
                throw new ArgumentNullException(nameof(templateData));
            }

            var notificationTemplate = await 
                _repository.SingleOrDefaultAsync(x => x.Abbreviation == templateData.TemplateAbbreviation);

            if (notificationTemplate == null)
            {
                throw new ArgumentException(
                    $"The notification template with abbreviation: {templateData.TemplateAbbreviation} does not exist in data store.");
            }

            return new NotificationDto
                {Body = GetBody(notificationTemplate.Body, templateData), Subject = notificationTemplate.Subject};
        }

        private string GetBody(string template, INotificationTemplateData templateData)
        {
            var body = new StringBuilder(template);

            templateData.GetKeys().ForEach(key => body = body.Replace(key, templateData.GetValue(key)));
            return body.ToString();
        }
    }
}

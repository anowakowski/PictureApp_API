using System;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using PictureApp.API.Services.NotificationTemplateData;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;
using PictureApp.API.Providers;

namespace PictureApp.API.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly IRepository<NotificationTemplate> _repository;
        private readonly IEmailClientProvider _emailClientProvider;

        public EmailNotificationService(IRepository<NotificationTemplate> repository,
            IEmailClientProvider emailClientProvider)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _emailClientProvider = emailClientProvider ?? throw new ArgumentNullException(nameof(emailClientProvider));
        }

        public async Task SendAsync(string recipient, INotificationTemplateData templateData)
        {
            // Attempt to get the template identified by an abbreviation
            var notificationTemplate = _repository.Find(x => x.Abbreviation == templateData.TemplateAbbreviation)
                .FirstOrDefault();
            if (notificationTemplate == null)
            {
                throw new EntityNotFoundException(
                    $"Can not find notification template with following abbreviation: {templateData.TemplateAbbreviation}");
            }
           
            // Prepare the email body
            var body = GetEmailBody(notificationTemplate.Body, templateData);

            // Send the email via gateway client
            await _emailClientProvider.SendAsync(new MailAddress(recipient), notificationTemplate.Subject, body);
        }

        private string GetEmailBody(string template, INotificationTemplateData templateData)
        {
            var body = new StringBuilder(template);

            templateData.GetKeys().ForEach(key => body.Replace(key, templateData.GetValue(key)));
            return body.ToString();
        }
    }
}

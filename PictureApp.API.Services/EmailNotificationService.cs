using System;
using System.Net.Mail;
using System.Threading.Tasks;
using PictureApp.API.Providers;
using PictureApp.Notifications;

namespace PictureApp.API.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly IEmailClientProvider _emailClientProvider;
        private readonly INotificationTemplateService _notificationTemplateService;

        public EmailNotificationService(IEmailClientProvider emailClientProvider, INotificationTemplateService notificationTemplateService)
        {
            _emailClientProvider = emailClientProvider ?? throw new ArgumentNullException(nameof(emailClientProvider));
            _notificationTemplateService = notificationTemplateService ?? throw new ArgumentNullException(nameof(notificationTemplateService));
        }

        public async Task SendAsync(string recipient, INotificationTemplateData templateData)
        {
            if (templateData == null)
            {
                throw new ArgumentNullException(nameof(templateData));
            }

            MailAddress recipientMailAddress;

            try
            {
                recipientMailAddress = new MailAddress(recipient);
            }
            catch (FormatException e)
            {
                throw new ArgumentException($"Incorrect format for passed recipient email: `{recipient}`.", e);
            }

            var notification = await _notificationTemplateService.CreateNotification(templateData);

            // Send the email via gateway client
            await _emailClientProvider.SendAsync(recipientMailAddress, notification.Subject, notification.Body);
        }
    }
}

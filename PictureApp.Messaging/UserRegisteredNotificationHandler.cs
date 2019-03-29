using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Services;
using PictureApp.Notifications;

namespace PictureApp.Messaging
{
    public class UserRegisteredNotificationHandler : INotificationHandler<UserRegisteredNotificationEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        
        public UserRegisteredNotificationHandler(INotificationService notificationService, IUserService userService, IConfiguration configuration)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task Handle(UserRegisteredNotificationEvent notification, CancellationToken cancellationToken)
        {
            // Get the user by his email
            var user = _userService.GetUser(notification.UserEmail);
            if (user == null)
            {
                throw new EntityNotFoundException($"The user with given email: {notification.UserEmail} does not exist in data store");
            }

            // Prepare activation uri
            var activationUriFormat = _configuration.GetSection("AppSettings:AccountActivationUriFormat").Value;
            var activationUri = Uri.EscapeUriString(activationUriFormat.Replace("{token}", user.ActivationToken));

            // Prepare notification with activation uri
            var templateData = new UserRegisteredNotificationTemplateData(activationUri, user.Username);
            await _notificationService.SendAsync(user.Email, templateData);                      
        }
    }
}

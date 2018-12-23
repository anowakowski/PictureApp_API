using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Models;
using PictureApp.API.Services;
using PictureApp.Notifications;

namespace PictureApp.Messaging
{
    public class UserRegisteredNotificationHandler : INotificationHandler<UserRegisteredNotificationEvent>
    {
        private INotificationService _notificationService;
        private IUserService _userService;
        private IRepository<NotificationTemplate> _repository;

        public UserRegisteredNotificationHandler(INotificationService notificationService, IUserService userService, IRepository<NotificationTemplate> repository)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task Handle(UserRegisteredNotificationEvent notification, CancellationToken cancellationToken)
        {
            var user = _userService.GetUser(notification.UserEmail);

            var templateData = new UserRegisteredNotificationTemplateData(user.ActivationToken, user.Username);
            // na podstawie emaila odczytanie id uzytkownika
            // odczytanie wlasciwego template'a do notyfikacji
            // wypelnienie template'a danymi


            // wyslanie maila do uzytkownika
            await _notificationService.SendAsync(user.Email, templateData);

            // jakie zaleznosci?
            // - INotificationService - wyslanie maila
            // - IRepository<NotificationTemplate> - odczytanie odpowiedniego template'a - a moze tutaj powinien byc odpowiedni serwis, ktory pozwala na odczytanie odpowiednich template'ow?            
        }
    }
}

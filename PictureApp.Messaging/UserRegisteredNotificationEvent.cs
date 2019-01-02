using MediatR;

namespace PictureApp.Messaging
{
    public class UserRegisteredNotificationEvent : INotification
    {
        public UserRegisteredNotificationEvent(string userEmail)
        {
            UserEmail = userEmail;
        }

        public string UserEmail { get; }
    }
}

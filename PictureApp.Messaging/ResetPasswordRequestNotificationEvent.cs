using MediatR;

namespace PictureApp.Messaging
{
    public class ResetPasswordRequestNotificationEvent : INotification
    {
        public ResetPasswordRequestNotificationEvent(string userEmail)
        {
            UserEmail = userEmail;
        }

        public string UserEmail { get; }
    }
}

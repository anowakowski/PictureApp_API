using MediatR;

namespace PictureApp.Messaging
{
    public class PhotoUploadedNotificationEvent : INotification
    {
        public string FileId { get; }

        public int UserId { get; }

        public PhotoUploadedNotificationEvent(string fileId, int userId)
        {
            FileId = fileId;
            UserId = userId;
        }
    }
}

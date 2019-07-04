using MediatR;

namespace PictureApp.Messaging
{
    public class PhotoUploadedNotificationEvent : INotification
    {
        public string FileId { get; }

        public PhotoUploadedNotificationEvent(string fileId)
        {
            FileId = fileId;
        }
    }
}

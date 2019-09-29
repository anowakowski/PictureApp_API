using MediatR;

namespace PictureApp.Messaging
{
    public class PhotoUploadedNotificationEvent : INotification
    {
        public string FileId { get; }

        public int UserId { get; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Description { get; set; }

        public PhotoUploadedNotificationEvent(string fileId, int userId, string title = "", string subtitle = "", string description = "")
        {
            FileId = fileId;
            UserId = userId;
            Title = title;
            Subtitle = subtitle;
            Description = description;
        }
    }
}

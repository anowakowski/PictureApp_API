namespace PictureApp.API.Models
{
    public class NotificationTemplate : Entity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string Abbreviation { get; set; }
    }
}

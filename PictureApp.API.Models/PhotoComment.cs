namespace PictureApp.API.Models
{
    public class PhotoComment : Entity
    {
        public string Content { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
        public Photo Photo { get; set; }
        public int PhotoId { get; set; }
    }
}
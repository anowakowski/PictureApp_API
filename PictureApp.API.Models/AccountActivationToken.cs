namespace PictureApp.API.Models
{
    public class AccountActivationToken : Entity // TODO: zmiana nazwy klasy na UserToken
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public string Token { get; set; }
    }
}

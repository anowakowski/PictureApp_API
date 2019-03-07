namespace PictureApp.API.Models
{
    public class AccountActivationToken : Entity, ITokenEntity
    {
        public int UserId { get; set; }

        public User User { get; set; }

        public string Token { get; set; }
    }
}

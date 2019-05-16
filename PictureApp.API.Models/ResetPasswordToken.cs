namespace PictureApp.API.Models
{
    public class ResetPasswordToken : Entity, ITokenEntity
    {
        public int UserId { get; set; }

        public User User { get; set; }

        public string Token { get; set; }
    }
}

namespace PictureApp.API.Dtos
{
    public class UserResetPasswordDto
    {
        public string Email { get; set; } // is it really necessary if email can be simply get from User static class?

        public string Password { get; set; }
    }
}

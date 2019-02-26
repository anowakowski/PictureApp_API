namespace PictureApp.API.Dtos
{
    public class UserForChangePasswordDto
    {
        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string RetypedPassword { get; set; }
    }
}

namespace PictureApp.API.Dtos.UserDto
{
    public class UserForEditProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }
        public string ActivationToken { get; set; }        
        
    }
}
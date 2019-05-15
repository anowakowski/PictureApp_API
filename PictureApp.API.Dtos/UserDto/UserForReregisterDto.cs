using System.ComponentModel.DataAnnotations;

namespace PictureApp.API.Dtos.UserDto
{
    public class UserForReregisterDto
    {
        [EmailAddress]
        public string Email { get; set; }
    }
}

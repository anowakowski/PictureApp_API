using System.ComponentModel.DataAnnotations;

namespace PictureApp.API.Dtos
{
    public class UserResetPasswordDto
    {
        public string Token { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "You must specify password between 4 and 8 characters")]
        public string Password { get; set; }
    }
}

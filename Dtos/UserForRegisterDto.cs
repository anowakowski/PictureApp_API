using System.ComponentModel.DataAnnotations;

namespace PictureApp_API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "You must specify password between 4 and 8 charackters")]
        public string Password { get; set; }
    }
}
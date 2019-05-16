using System.ComponentModel.DataAnnotations;

namespace PictureApp.API.Dtos
{
    public class UserForChangePasswordDto
    {
        public string OldPassword { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "You must specify password between 4 and 8 characters")]
        public string NewPassword { get; set; }

        public string RetypedPassword { get; set; }
    }
}

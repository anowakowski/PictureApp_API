using System.ComponentModel.DataAnnotations;

namespace PictureApp.API.Dtos
{
    public class UserForReregisterDto
    {
        [EmailAddress]
        public string Email { get; set; }
    }
}

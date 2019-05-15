using System.Collections.Generic;
using PictureApp.API.Dtos.PhotosDto;

namespace PictureApp.API.Dtos.UserDto
{
    public class UserForEditProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public IEnumerable<PhotosForPhotoExploreViewDto> Photos { get; set; }      
    }
}
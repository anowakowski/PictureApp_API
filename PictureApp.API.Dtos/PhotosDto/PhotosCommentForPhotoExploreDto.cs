using PictureApp.API.Dtos.UserDto;

namespace PictureApp.API.Dtos.PhotosDto
{
    public class PhotosCommentForPhotoExploreDto
    {
        public string Content { get; set; }
        public UserForDetailedDto User { get; set; }
    }
}
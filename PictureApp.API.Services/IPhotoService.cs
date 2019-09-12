using System.Collections.Generic;
using System.Threading.Tasks;
using PictureApp.API.Dtos.PhotosDto;
using PictureApp.API.Dtos.UserDto;

namespace PictureApp.API.Services
{
    public interface IPhotoService
    {
        Task SetUsersPhotosWithComments(IEnumerable<UsersListWithFollowersForExploreDto> users);

        Task AddPhotoForUser(PhotoForUserDto photoForUser);

        Task UpdatePhotoForUser(PhotoForUserDto photoForUser);

        Task RemovePhoto(int userId, string fileId);
    }
}
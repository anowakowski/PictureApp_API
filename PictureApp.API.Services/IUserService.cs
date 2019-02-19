using System.Collections.Generic;
using System.Threading.Tasks;
using PictureApp.API.Dtos;
using PictureApp.API.Dtos.UserDto;

namespace PictureApp.API.Services
{
    public interface IUserService
    {
        Task<UserForDetailedDto>  GetUser(int userId);
        UserForDetailedDto GetUser(string email);
        Task<UserForEditProfileDto> GetUserForEdit(int userId);
        Task<IEnumerable<UsersListWithFollowersForExploreDto>> GetAllWithFollowers(int userId);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using PictureApp.API.Dtos;

namespace PictureApp.API.Services
{
    public interface IUserService
    {
        UserForDetailedDto GetUser(int userId);
        Task<IEnumerable<UsersListWithFollowersForExploreDto>> GetAllWithFollowers(int userId);

        UserForDetailedDto GetUser(string email);
    }
}
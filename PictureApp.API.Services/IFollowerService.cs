using System.Collections.Generic;
using System.Threading.Tasks;
using PictureApp.API.Dtos;

namespace PictureApp.API.Services
{
    public interface IFollowerService
    {
        Task SetUpFollower(int userId, int recipientId);
        Task SetUpUnfollower(int userId, int recipientId);
        Task<IEnumerable<UsersListWithFollowersForExploreDto>> GetAllWithFollowers(int userId);
    }
}
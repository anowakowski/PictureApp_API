using System.Threading.Tasks;
using PictureApp.API.Dtos;

namespace PictureApp.API.Services
{
    public interface IFollowerService
    {
        Task SetUpFollower(int userId, int recipientId);
        Task<UsersListForDiscoverDto> GetUnFollowedUsers(int userId);
    }
}
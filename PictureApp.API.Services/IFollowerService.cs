using System.Collections.Generic;
using System.Threading.Tasks;
using PictureApp.API.Dtos;
using PictureApp.API.Models;

namespace PictureApp.API.Services
{
    public interface IFollowerService
    {
        Task SetUpFollower(int userId, int recipientId);
        Task SetUpUnfollower(int userId, int recipientId);
        Task<IEnumerable<UserFollower>> GetFollowers(int userId);
    }
}
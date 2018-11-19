using System.Collections.Generic;
using System.Threading.Tasks;
using PictureApp.API.Dtos;
using PictureApp.API.Models;

namespace PictureApp_API.Services
{
    public interface IUserService
    {
        Task<UserForDetailedDto> GetUser(int userId);
        Task<IEnumerable<UserForExploreDto>> GetUsersWithPhotos();
    }
}
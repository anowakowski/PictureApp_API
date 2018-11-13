using System.Threading.Tasks;
using PictureApp.API.Models;
using PictureApp_API.Dtos;

namespace PictureApp_API.Services
{
    public interface IUserService
    {
        Task<UserForDetailedDto> GetUser(int userId);
         
    }
}
using System.Threading.Tasks;
using PictureApp.API.Dtos;

namespace PictureApp.API.Services
{
    public interface IUserService
    {
        Task<UserForDetailedDto> GetUser(int userId);
    }
}
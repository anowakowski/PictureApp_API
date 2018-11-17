using System.Threading.Tasks;
using PictureApp.API.Dtos;
using PictureApp.API.Models;

namespace PictureApp.API.Services
{
    public interface IAuthService
    {
        void Register(UserForRegisterDto userForRegister);
        Task<UserLoggedInDto> Login(string email, string password);
        UserLoggedInDto GetLoggedInUser(User user);
        Task<bool> UserExists(string email);
    }
}

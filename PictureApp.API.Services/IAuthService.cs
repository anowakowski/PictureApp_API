using System.Threading.Tasks;
using PictureApp.API.Dtos;
using PictureApp.API.Dtos.UserDto;

namespace PictureApp.API.Services
{
    public interface IAuthService
    {
        Task Register(UserForRegisterDto userForRegister);

        Task Reregister(UserForReregisterDto userForReregister);

        Task Activate(string token);

        Task<UserLoggedInDto> Login(string email, string password);

        Task<bool> UserExists(string email);
    }
}
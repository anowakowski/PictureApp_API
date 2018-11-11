using System.Threading.Tasks;
using PictureApp.API.Dtos;

namespace PictureApp.API.Services
{
    public interface IAuthService
    {
        void Register(UserForRegisterDto userForRegister);
        void Login(string email, string password);
        UserLoggedInDto GetLoggedInUser(string email);
        Task<bool> UserExists(string email);
    }
}

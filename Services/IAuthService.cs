using System.Threading.Tasks;
using PictureApp.API.Dtos;

namespace PictureApp.API.Services
{
    public interface IAuthService
    {
        void Register(UserForRegisterDto userForRegister, string password);
        void Login(string username, string password);
        Task<bool> UserExists(string username);
    }
}

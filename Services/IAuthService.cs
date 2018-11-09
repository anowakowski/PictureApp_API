using System.Threading.Tasks;
using PictureApp.API.Models;

namespace PictureApp.API.Services
{
    public interface IAuthService
    {
        Task<User> Register(User user, string password);
        Task<User> Login(string username, string password);
        Task<bool> UserExists(string username);
    }
}

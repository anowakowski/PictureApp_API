using System.Threading.Tasks;
using PictureApp_API.Models;

namespace PictureApp_API.Data.Repository
{
    public interface IAuthRepository
    {
         Task<User> Register(User user, string password);
    }
}
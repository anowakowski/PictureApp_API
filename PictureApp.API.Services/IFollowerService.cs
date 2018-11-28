using System.Threading.Tasks;

namespace PictureApp.API.Services
{
    public interface IFollowerService
    {
        Task SetUpFollower(int userId, int recipientId);
    }
}
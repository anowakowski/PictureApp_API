using PictureApp.API.Dtos;

namespace PictureApp.API.Services
{
    public interface IUserService
    {
        UserForDetailedDto GetUser(int userId);

        UserForDetailedDto GetUser(string email);
    }
}
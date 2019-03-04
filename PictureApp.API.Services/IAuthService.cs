using System.Threading.Tasks;
using PictureApp.API.Dtos;

namespace PictureApp.API.Services
{
    public interface IAuthService
    {
        Task Register(UserForRegisterDto userForRegister);

        Task Activate(string token);

        Task ChangePassword(string email, string oldPassword, string newPassword, string retypedNewPassword);

        Task ResetPasswordRequest(string email);

        Task ResetPassword(string token, string newPassword);

        Task<UserLoggedInDto> Login(string email, string password);

        Task<bool> UserExists(string email);
    }
}
using System.Security.Claims;

namespace PictureApp.API.Providers
{
    public interface IAuthTokenProvider
    {
        string GetToken(params Claim[] claims);
    }
}

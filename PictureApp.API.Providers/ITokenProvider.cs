namespace PictureApp.API.Providers
{
    public interface ITokenProvider
    {
        string CreateToken();

        bool IsTokenExpired(string token, int expirationTimeInHours);
    }
}

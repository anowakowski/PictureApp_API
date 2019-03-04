namespace PictureApp.API.Providers
{
    public interface IActivationTokenProvider // TODO: change name to ITokenProvider
    {
        string CreateToken();

        bool IsTokenExpired(string token);
    }
}

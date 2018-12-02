using System;

namespace PictureApp.API.Providers
{
    public interface IActivationTokenProvider
    {
        string CreateToken();

        bool IsTokenExpired(string token);
    }
}

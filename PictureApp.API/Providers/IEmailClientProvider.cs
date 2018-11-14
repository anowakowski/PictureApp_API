namespace PictureApp.API.Providers
{
    public interface IEmailClientProvider
    {
        void Send(string to, string subject, string body);
    }
}

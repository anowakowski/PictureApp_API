using System.Threading.Tasks;

namespace PictureApp.API.Providers
{
    public interface IEmailClientProvider
    {
        Task SendAsync(string to, string subject, string body);
    }
}

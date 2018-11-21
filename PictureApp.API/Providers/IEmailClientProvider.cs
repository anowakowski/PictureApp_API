using System.Net.Mail;
using System.Threading.Tasks;

namespace PictureApp.API.Providers
{
    public interface IEmailClientProvider
    {
        Task SendAsync(MailAddress to, string subject, string body);
    }
}

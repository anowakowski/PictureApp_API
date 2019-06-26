using System.IO;
using System.Threading.Tasks;

namespace PictureApp.API.Services
{
    public interface IFileUploadService
    {
        Task Upload(Stream file, long userId);
    }
}

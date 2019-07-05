using System.IO;
using System.Threading.Tasks;

namespace PictureApp.API.Providers
{
    public interface IFilesStorageProvider
    {
        Task<FileUploadResult> Upload(Stream fileStream, string fileId);

        Task Remove(string fileId);

        Task<FileDownloadResult> Download(string fileId);
    }
}

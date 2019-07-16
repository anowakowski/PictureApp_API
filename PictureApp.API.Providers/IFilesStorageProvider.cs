using System.IO;
using System.Threading.Tasks;

namespace PictureApp.API.Providers
{
    public interface IFilesStorageProvider
    {
        Task<FileUploadResult> UploadAsync(Stream fileStream, string fileId);

        Task Remove(string fileId);

        Task<FileDownloadResult> DownloadAsync(string fileId);
    }
}

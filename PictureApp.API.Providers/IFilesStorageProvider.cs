using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PictureApp.API.Providers
{
    public interface IFilesStorageProvider
    {
        Task<FileUploadResult> UploadAsync(Stream fileStream, string fileId, string folder = null);

        Task Remove(string fileId, string folder = null);

        Task<FileDownloadResult> DownloadAsync(string fileId, string folder = null);

        Task<IEnumerable<FileItemResult>> GetFiles(string folder);

        string CreateContainerName(string postfix);
    }
}

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PictureApp.API.Dtos.PhotosDto;

namespace PictureApp.API.Providers
{
    public interface IFilesStorageProvider
    {
        Task<FileUploadResult> UploadAsync(Stream fileStream, PhotoForStreamUploadMetadataDto fileMetadata, string folder = null);

        Task Remove(string fileId, string folder = null);

        Task<FileDownloadResult> DownloadAsync(string fileId, string folder = null);

        Task<IEnumerable<FileItemResult>> GetFiles(string folder);

        string CreateContainerName(string postfix);

        string CreateFileName(PhotoForStreamUploadMetadataDto fileMetadata);
    }
}
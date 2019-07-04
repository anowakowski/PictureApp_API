using System.IO;

namespace PictureApp.API.Providers
{
    public interface IFilesStorageProvider
    {
        FileUploadResult Upload(Stream fileStream, string fileId);

        void Remove(string fileId);
    }
}

using System.IO;

namespace PictureApp.API.Providers
{
    public interface IPhotoStorageProvider
    {
        ImageUploadResult Upload(string fileName, Stream fileData);
    }
}

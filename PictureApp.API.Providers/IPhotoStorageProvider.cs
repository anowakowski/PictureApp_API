using System.IO;
using System.Threading.Tasks;

namespace PictureApp.API.Providers
{
    public interface IPhotoStorageProvider
    {
        Task<ImageUploadResult> Upload(string fileName, Stream fileData);
    }
}

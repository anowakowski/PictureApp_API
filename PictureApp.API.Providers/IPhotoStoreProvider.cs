using System.IO;

namespace PictureApp.API.Providers
{
    public interface IPhotoStoreProvider
    {
        void Upload(string fileName, Stream fileData);
    }
}

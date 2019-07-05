using System.IO;

namespace PictureApp.API.Providers
{
    public class FileDownloadResult
    {
        public Stream FileStream { get; private set; }

        private FileDownloadResult()
        {
        }

        public static FileDownloadResult Create(Stream fileStream)
        {
            return new FileDownloadResult
            {
                FileStream = fileStream
            };
        }
    }
}

using System.IO;

namespace PictureApp.API.Providers
{
    public class FileDownloadResult
    {
        public string FileId { get; private set; }

        public Stream FileStream { get; private set; }

        private FileDownloadResult()
        {
        }

        public static FileDownloadResult Create(Stream fileStream, string fileId)
        {
            return new FileDownloadResult
            {
                FileId = fileId,
                FileStream = fileStream
            };
        }
    }
}

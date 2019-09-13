using System;

namespace PictureApp.API.Providers
{
    public class FileUploadResult
    {
        public string FileId { get; private set; }

        public Uri Uri { get; private set; }

        private FileUploadResult()
        {
        }

        public static FileUploadResult Create(string fileId, Uri uri)
        {
            return new FileUploadResult
            {
                FileId = fileId,
                Uri = uri
            };
        }
    }
}

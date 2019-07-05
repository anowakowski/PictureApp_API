using System;

namespace PictureApp.API.Providers
{
    public class FileUploadResult
    {
        public Uri Uri { get; private set; }

        private FileUploadResult()
        {
        }

        public static FileUploadResult Create(Uri uri)
        {
            return new FileUploadResult
            {
                Uri = uri                
            };
        }
    }
}

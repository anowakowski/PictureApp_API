using System;

namespace PictureApp.API.Providers
{
    public class ImageUploadResult
    {
        public Uri Uri { get; private set; }

        public Uri SecureUri { get; private set; }

        public string Format { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public string PublicId { get; private set; }

        public long Length { get; private set; }

        public string Signature { get; private set; }

        public string Version { get; private set; }

        private ImageUploadResult()
        {
        }

        public static ImageUploadResult Create(Uri uri, Uri secureUri, string format, DateTime createdAt, string publicId, long length, string signature, string version)
        {
            return new ImageUploadResult
            {
                Uri = uri,
                SecureUri = secureUri,
                Format = format,
                CreatedAt = createdAt,
                PublicId = publicId,
                Length = length,
                Signature = signature,
                Version = version
            };
        }
    }
}

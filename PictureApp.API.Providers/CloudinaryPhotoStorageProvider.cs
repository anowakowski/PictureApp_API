﻿using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using PictureApp.API.Extensions.Extensions;

namespace PictureApp.API.Providers
{
    public class CloudinaryPhotoStorageProvider : IPhotoStorageProvider
    {
        private readonly IConfiguration _configuration;

        public CloudinaryPhotoStorageProvider(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<ImageUploadResult> Upload(string fileName, Stream fileData)
        {
            var cloudinary = CloudinaryInstance();

            fileData.ResetPosition();
            var fileDescription = new FileDescription(fileName, fileData);

            var imageUploadParameters = new ImageUploadParams
            {
                File = fileDescription
            };

            var uploadResult = cloudinary.Upload(imageUploadParameters);

            return ImageUploadResult.Create(uploadResult.Uri, uploadResult.SecureUri, uploadResult.Format,
                uploadResult.CreatedAt, uploadResult.PublicId, uploadResult.Length, uploadResult.Signature,
                uploadResult.Version);
        }

        private Cloudinary CloudinaryInstance()
        {
            var account = new Account
            {
                Cloud = _configuration.GetSection("CloudinaryStorage:CloudName").Value,
                ApiKey = _configuration.GetSection("CloudinaryStorage:ApiKey").Value,
                ApiSecret = _configuration.GetSection("CloudinaryStorage:ApiSecret").Value
            };

            return new Cloudinary(account);
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Models;
using PictureApp.API.Providers;

namespace PictureApp.API.Services
{
    public class FileUploadService : IFileUploadService
    {
        private IRepository<Photo> _photoRepository;
        private IPhotoStorageProvider _photoStorageProvider;

        public FileUploadService(IRepository<Photo> photoRepository, IPhotoStorageProvider photoStorageProvider)
        {
            _photoRepository = photoRepository ?? throw new ArgumentNullException(nameof(photoRepository));
            _photoStorageProvider = photoStorageProvider ?? throw new ArgumentNullException(nameof(photoStorageProvider));
        }

        public Task Upload(Stream file, long userId)
        {
            // Responsibility?
            // - save file in database as a temporary resource (need to extend Photo entity or design a special table for this purpose)
            // - send file to the cloud repository
            // - when the file is successfully uploaded in cloud get a metadata and update state in database
            // - remove file from the database

            throw new NotImplementedException();
        }
    }
}

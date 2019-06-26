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
        private IPhotoStoreProvider _photoStoreProvider;

        public FileUploadService(IRepository<Photo> photoRepository, IPhotoStoreProvider photoStoreProvider)
        {
            _photoRepository = photoRepository ?? throw new ArgumentNullException(nameof(photoRepository));
            _photoStoreProvider = photoStoreProvider ?? throw new ArgumentNullException(nameof(photoStoreProvider));
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

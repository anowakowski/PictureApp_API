using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PictureApp.API.Dtos.PhotosDto;
using PictureApp.API.Providers;
using PictureApp.API.Services;

namespace PictureApp.Messaging
{
    public class PhotoUploadedNotificationHandler : INotificationHandler<PhotoUploadedNotificationEvent>
    {
        private readonly IPhotoStorageProvider _photoStorageProvider;
        private readonly IPhotoService _photoService;
        private readonly IFilesStorageProvider _filesStorageProvider;

        public PhotoUploadedNotificationHandler(IPhotoStorageProvider photoStorageProvider, IPhotoService photoService, IFilesStorageProvider filesStorageProvider)
        {
            _photoStorageProvider = photoStorageProvider ?? throw new ArgumentNullException(nameof(photoStorageProvider));
            _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));
            _filesStorageProvider = filesStorageProvider ?? throw new ArgumentNullException(nameof(filesStorageProvider));
        }

        public async Task Handle(PhotoUploadedNotificationEvent notification, CancellationToken cancellationToken)
        {
            var downloadResult = await _filesStorageProvider.DownloadAsync(notification.FileId);
            var imageUploadResult = await _photoStorageProvider.Upload(notification.FileId, downloadResult.FileStream);
            var photoForUser = new PhotoForUserDto
            {
                UserId = notification.UserId,
                Url = imageUploadResult.Uri
            };
            await _photoService.UpdatePhotoForUser(photoForUser);
            await _filesStorageProvider.Remove(notification.FileId);

            // Responsibility ?
            // - get file from temporary storage [IFilesStorageProvider]
            // - upload file to the cloudinary [IPhotoStorageProvider]
            // - when the file is successfully uploaded in cloud get a metadata and update state in database [IPhotoService]
            // - remove file from the datastore mentioned above [IFilesStorageProvider]
        }
    }
}

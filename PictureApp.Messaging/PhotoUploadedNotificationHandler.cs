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
                FileId = notification.FileId,
                UserId = notification.UserId,
                Url = imageUploadResult.Uri,
                Title = notification.Title,
                Subtitle = notification.Subtitle,
                Description = notification.Description
            };
            await _photoService.UpdatePhotoForUser(photoForUser);
            await _filesStorageProvider.Remove(notification.FileId);
        }
    }
}
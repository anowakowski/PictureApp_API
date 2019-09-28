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
        private readonly IPhotoServiceScoped _photoService;
        private readonly IFilesStorageProvider _filesStorageProvider;
        private readonly IUserService _userService;

        public PhotoUploadedNotificationHandler(IUserService userService, IPhotoStorageProvider photoStorageProvider,
            IPhotoServiceScoped photoService, IFilesStorageProvider filesStorageProvider)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _photoStorageProvider = photoStorageProvider ?? throw new ArgumentNullException(nameof(photoStorageProvider));
            _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));
            _filesStorageProvider = filesStorageProvider ?? throw new ArgumentNullException(nameof(filesStorageProvider));
        }

        public async Task Handle(PhotoUploadedNotificationEvent notification, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUser(notification.UserId);
            var downloadResult = await _filesStorageProvider.DownloadAsync(notification.FileId, user.PendingUploadPhotosFolderName);
            var imageUploadResult = await _photoStorageProvider.Upload(notification.FileId, downloadResult.FileStream);
            var photoForUser = new PhotoForUserDto
            {
                FileId = notification.FileId,
                PublicId = imageUploadResult.PublicId,
                UserId = notification.UserId,
                Url = imageUploadResult.Uri,
                Title = notification.Title,
                Subtitle = notification.Subtitle,
                Description = notification.Description
            };

            await _photoService.UpdatePhotoForUser(photoForUser);
            await _filesStorageProvider.Remove(notification.FileId, user.PendingUploadPhotosFolderName);
        }
    }
}
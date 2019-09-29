using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Dtos.PhotosDto;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Providers;
using PictureApp.API.Services;
using PictureApp.Messaging;

namespace PictureApp.API.Tests.Messaging
{
    [TestFixture]
    public class PhotoUploadedNotificationHandlerTests : GuardClauseAssertionTests<PhotoUploadedNotificationHandler>
    {
        [Test]
        public async Task Handle_WhenCalled_AttemptToUpdatePhotoForUserAndRemoveItFromFilesStorageExpected()
        {
            // ARRANGE
            var filesStorageProvider = Substitute.For<IFilesStorageProvider>();
            var fileDownloadResult = FileDownloadResult.Create(new MemoryStream(), "aa37acdc7bbf4260922a25066948db9e");
            filesStorageProvider.DownloadAsync(fileDownloadResult.FileId, Arg.Any<string>()).Returns(fileDownloadResult);
            var photoStorageProvider = Substitute.For<IPhotoStorageProvider>();
            var imageUploadResult = ImageUploadResult.Create(new Uri(@"https:\\veileye.com"),
                new Uri(@"https:\\veileye.com"), "jpg", DateTime.Now, "publicId", 1, "signature", "version");
            photoStorageProvider.Upload(fileDownloadResult.FileId, fileDownloadResult.FileStream)
                .Returns(imageUploadResult);
            var photoService = Substitute.For<IPhotoServiceScoped>();
            var userService = Substitute.For<IUserService>();
            userService.GetUser(Arg.Any<int>()).Returns(new UserForDetailedDto());

            var sut = new PhotoUploadedNotificationHandler(userService, photoStorageProvider, photoService, filesStorageProvider);
            var photoUploadedNotificationEvent = new PhotoUploadedNotificationEvent(fileDownloadResult.FileId, 99);
            var expectedPhotoForUser = new PhotoForUserDto
            {
                FileId = photoUploadedNotificationEvent.FileId,
                PublicId = imageUploadResult.PublicId,
                UserId = photoUploadedNotificationEvent.UserId,
                Url = imageUploadResult.Uri,
                Title = photoUploadedNotificationEvent.Title,
                Subtitle = photoUploadedNotificationEvent.Subtitle,
                Description = photoUploadedNotificationEvent.Description
            };
            PhotoForUserDto actualPhotoForUser = null;
            photoService.When(x => x.UpdatePhotoForUser(Arg.Any<PhotoForUserDto>()))
                .Do(x => actualPhotoForUser = x.ArgAt<PhotoForUserDto>(0));

            // ACT
            await sut.Handle(photoUploadedNotificationEvent, new CancellationToken());

            // ASSERT
            actualPhotoForUser.Should().BeEquivalentTo(expectedPhotoForUser);
            await filesStorageProvider.Received().Remove(photoUploadedNotificationEvent.FileId);
        }
    }
}
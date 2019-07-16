using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
using PictureApp.API.Dtos.PhotosDto;
using PictureApp.API.Providers;
using PictureApp.API.Services;
using PictureApp.Messaging;

namespace PictureApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IFilesStorageProvider _filesStorageProvider;
        private readonly IMediator _mediator;
        private readonly IPhotoService _photoService;


        public UploadController(IUserService userService, IFilesStorageProvider filesStorageProvider, IMediator mediator, IPhotoService photoService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _filesStorageProvider = filesStorageProvider ?? throw new ArgumentNullException(nameof(filesStorageProvider));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));
        }

        [HttpPost("upload"), DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFile(PhotosForUploadDto photosForUpload)
        {
            var userEmail = User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var user = _userService.GetUser(userEmail);

            photosForUpload.Files.ForEach(async x =>
            {
                Stream stream = new MemoryStream();
                await x.CopyToAsync(stream);
                var fileMetadata = photosForUpload.Metadata.Single(file => file.FileId == x.FileName);
                var fileUploadResult = await _filesStorageProvider.UploadAsync(stream, x.FileName);
                var photoForUser = new PhotoForUserDto
                {
                    UserId = user.Id,
                    Title = fileMetadata.Title,
                    Description = fileMetadata.Description,
                    Subtitle = fileMetadata.Subtitle,
                    Url = fileUploadResult.Uri
                };
                await _photoService.AddPhotoForUser(photoForUser);
                var @event = new PhotoUploadedNotificationEvent(fileMetadata.FileId, user.Id);
                await _mediator.Publish(@event);
            });

            return StatusCode(StatusCodes.Status201Created);

            // Responsibility ?
            // - save file in temporary datastore (do it by publishing?) [IFilesStorageProvider]
            // - put metadata in local database [IPhotoService]
            // - publish message that there is uploaded file [IMediator]
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos.PhotosDto;
using PictureApp.API.Models;
using PictureApp.API.Services;

namespace PictureApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IUserService _userService;

        public UploadController(IFileUploadService fileUploadService, IUserService userService)
        {
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }
        
        [HttpPost("upload"), DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFile(PhotosForUploadDto photosForUpload)
        {
            var userEmail = User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var user = _userService.GetUser(userEmail);

            photosForUpload.Files.ForEach(x =>
            {
                Stream stream = new MemoryStream();
                x.CopyToAsync(stream);
                _fileUploadService.Upload(stream, user.Id);
            });

            return StatusCode(StatusCodes.Status201Created);

            // Responsibility of upload controller
            // - save file in temporary datastore (do it by publishing?) [IFilesStorageProvider]
            // - put metadata in local database [IPhotoService]
            // - publish message that there is uploaded file [IMediator]

            // Responsibility of uploaded file message handler
            // - upload file to the cloudinary [IPhotoStorageProvider]
            // - when the file is successfully uploaded in cloud get a metadata and update state in database [IPhotoService]
            // - remove file from the datastore mentioned above [IFilesStorageProvider]
        }
    }
}
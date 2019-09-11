﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using MoreLinq;
using PictureApp.API.Dtos.PhotosDto;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Filters;
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
        private readonly IFileFormatInspectorProvider _fileFormatInspectorProvider;
        private readonly IConfiguration _configuration;
        private static readonly FormOptions DefaultFormOptions = new FormOptions();

        public UploadController(IUserService userService, IFilesStorageProvider filesStorageProvider,
            IMediator mediator, IPhotoService photoService, IFileFormatInspectorProvider fileFormatInspectorProvider, IConfiguration configuration)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _filesStorageProvider = filesStorageProvider ?? throw new ArgumentNullException(nameof(filesStorageProvider));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));
            _fileFormatInspectorProvider = fileFormatInspectorProvider ?? throw new ArgumentNullException(nameof(fileFormatInspectorProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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

            return NoContent();

            // Responsibility ?
            // - save file in temporary datastore (do it by publishing?) [IFilesStorageProvider]
            // - put metadata in local database [IPhotoService]
            // - publish message that there is uploaded file [IMediator]
        }

        [HttpPost("uploadStream"), DisableRequestSizeLimit]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadStreamFile()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            var formAccumulator = new KeyValueAccumulator();

            var boundary = MultipartRequestHelper.GetBoundary(
                    MediaTypeHeaderValue.Parse(Request.ContentType),
                    DefaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);

            MemoryStream fileStream = null;
            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        fileStream = new MemoryStream();
                        await section.Body.CopyToAsync(fileStream);
                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        // Content-Disposition: form-data; name="key"
                        //
                        // value

                        // Do not limit the key name length here because the 
                        // multipart headers length limit is already in effect.
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                        var encoding = GetEncoding(section);
                        using (var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true))
                        {
                            // The value length limit is enforced by MultipartBodyLengthLimit
                            var value = await streamReader.ReadToEndAsync();
                            if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = string.Empty;
                            }

                            formAccumulator.Append(key.ToString(), value);

                            if (formAccumulator.ValueCount > DefaultFormOptions.ValueCountLimit)
                            {
                                throw new InvalidDataException($"Form key count limit {DefaultFormOptions.ValueCountLimit} exceeded.");
                            }
                        }
                    }
                }

                section = await reader.ReadNextSectionAsync();
            };

            if (!_fileFormatInspectorProvider.ValidateFileFormat(fileStream))
            {
                return BadRequest("This file format is not supported");
            }

            var fileMetadata = new PhotoForStreamUploadMetadataDto();
            var formValueProvider = new FormValueProvider(
                BindingSource.Form,
                new FormCollection(formAccumulator.GetResults()), CultureInfo.CurrentCulture);

            var bindingSuccessful = await TryUpdateModelAsync(fileMetadata, prefix: string.Empty,
                valueProvider: formValueProvider);
            if (!bindingSuccessful && !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = GetUser();
            var fileUploadResult = await _filesStorageProvider.UploadAsync(fileStream,
                string.Format(_configuration.GetSection("AzureCloud:FileNameFormat").Value, fileMetadata.FileId,
                    fileMetadata.FileExtension), user.PendingUploadPhotosFolderName);
            var photoForUser = new PhotoForUserDto
            {
                FileId = fileMetadata.FileId,
                UserId = user.Id,
                Url = fileUploadResult.Uri
            };
            await _photoService.AddPhotoForUser(photoForUser);

            return NoContent();
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
            // most cases.
            return !hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding) ? Encoding.UTF8 : mediaType.Encoding;
        }

        [HttpPost("confirmPendingUploads")]
        public async Task<IActionResult> ConfirmPendingUploads(PhotoForUploadMetadataDto[] pendingFilesMetadata)
        {            
            var user = GetUser();
            await Task.Run(() => pendingFilesMetadata.ToList().ForEach(async x =>
            {
                var @event = new PhotoUploadedNotificationEvent(x.FileId, user.Id, x.Title, x.Subtitle, x.Description);
                await _mediator.Publish(@event);
            }));

            return NoContent();
        }

        [HttpGet("getPendingUploads")]
        public async Task<IActionResult> GetPendingUploads()
        {
            var user = GetUser();
            var pendingUploadFiles = await _filesStorageProvider.GetFiles(user.PendingUploadPhotosFolderName);

            return Ok(pendingUploadFiles.Select(x => new PhotoDto() {FileName = x.FileName}));
        }

        [HttpDelete("removePendingUploads/{ids}")]
        public async Task<IActionResult> RemovePendingUploads(string ids)
        {
            var user = GetUser();
            var pendingUploadsToRemoveIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries);
            await Task.Run(() => pendingUploadsToRemoveIds.ForEach(async x =>
                await _filesStorageProvider.Remove(x, user.PendingUploadPhotosFolderName)));

            // TODO: remove files from local database -> IPhotoService.RemovePhotoFromUser

            return NoContent();            
        }

        private UserForDetailedDto GetUser()
        {
            var userEmail = User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            return _userService.GetUser(userEmail);
        }
    }
}
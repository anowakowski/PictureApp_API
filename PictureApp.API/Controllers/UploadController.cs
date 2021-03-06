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
using PictureApp.API.Extensions;
using PictureApp.API.Extensions.Extensions;
using PictureApp.API.Filters;
using PictureApp.API.Helpers;
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
        private static readonly FormOptions DefaultFormOptions = new FormOptions();

        public UploadController(IUserService userService, IFilesStorageProvider filesStorageProvider,
            IMediator mediator, IPhotoService photoService, IFileFormatInspectorProvider fileFormatInspectorProvider, IConfiguration configuration)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _filesStorageProvider = filesStorageProvider ?? throw new ArgumentNullException(nameof(filesStorageProvider));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));
            _fileFormatInspectorProvider = fileFormatInspectorProvider ?? throw new ArgumentNullException(nameof(fileFormatInspectorProvider));
        }

        [HttpPost("uploadStream"), DisableRequestSizeLimit]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadStreamFile()
        {
            if (!Helpers.MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            var file = await GetFileStream(HttpContext.Request.Body);

            if (!_fileFormatInspectorProvider.ValidateFileFormat(file.stream))
            {
                return BadRequest("Given file format is not supported");
            }

            var fileMetadata = new PhotoForStreamUploadMetadataDto();
            var bindingSuccessful = await TryUpdateModelAsync(fileMetadata, prefix: string.Empty,
                valueProvider: file.formValueProvider);
            if (!bindingSuccessful && !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = GetUser();
            var fileUploadResult = await _filesStorageProvider.UploadAsync(file.stream, fileMetadata, user.PendingUploadPhotosFolderName);
            var photoForUser = new PhotoForUserDto
            {
                FileId = fileUploadResult.FileId,
                UserId = user.Id,
                Url = fileUploadResult.Uri
            };
            await _photoService.AddPhotoForUser(photoForUser);

            return NoContent();
        }

        [HttpPost("confirmPendingUploads")]
        public async Task<IActionResult> ConfirmPendingUploads([FromBody] PhotoForUploadMetadataDto[] pendingFilesMetadata)
        {
            var user = GetUser();

            var pendingUploadFiles = await _filesStorageProvider.GetFiles(user.PendingUploadPhotosFolderName);
            if (!pendingUploadFiles.Any())
            {
                return BadRequest("Attempt to confirm pending uploads failed because there are no awaiting uploads");
            }

            var pendingUploadsToConfirmIds = pendingFilesMetadata.Select(x => _filesStorageProvider.CreateFileName(
                new PhotoForStreamUploadMetadataDto
                {
                    FileId = x.FileId,
                    FileExtension = x.FileExtension
                }));
            var pendingUploadsToConfirm = pendingUploadFiles.Select(x => x.FileId)
                .Intersect(pendingUploadsToConfirmIds.AsEnumerable()).ToList();
            if (!pendingUploadsToConfirm.Any())
            {
                return BadRequest("Attempt to confirm pending uploads failed because of wrong passed ids");
            }

            await Task.Run(() => pendingFilesMetadata.ToList().ForEach(async x =>
            {
                var fileName = _filesStorageProvider.CreateFileName(new PhotoForStreamUploadMetadataDto
                {
                    FileId = x.FileId,
                    FileExtension = x.FileExtension
                });
                var @event = new PhotoUploadedNotificationEvent(fileName, user.Id, x.Title, x.Subtitle, x.Description);
                await _mediator.Publish(@event);
            }));

            return NoContent();
        }

        [HttpGet("getPendingUploads")]
        public async Task<IActionResult> GetPendingUploads()
        {
            var user = GetUser();
            var pendingUploadFiles = await _filesStorageProvider.GetFiles(user.PendingUploadPhotosFolderName);

            return Ok(pendingUploadFiles.Select(x => new PhotoDto() { FileId = x.FileId }));
        }

        [HttpDelete("removePendingUploads/{ids}")]
        public async Task<IActionResult> RemovePendingUploads(string ids)
        {
            var user = GetUser();

            var pendingUploadFiles = await _filesStorageProvider.GetFiles(user.PendingUploadPhotosFolderName);
            if (!pendingUploadFiles.Any())
            {
                return BadRequest("Attempt to remove pending uploads failed because there are no pending uploads");
            }

            var pendingUploadsToRemoveIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries);                        
            var pendingUploadsToRemove = pendingUploadFiles.Select(x => x.FileId)
                .Intersect(pendingUploadsToRemoveIds.AsEnumerable()).ToList();

            if (!pendingUploadsToRemove.Any())
            {
                return BadRequest("Attempt to remove pending uploads failed because of wrong passed ids");
            }

            if (pendingUploadsToRemoveIds.AsEnumerable().Except(pendingUploadFiles.Select(x => x.FileId)).Any())
            {
                return BadRequest("Attempt to remove pending uploads failed because there are some not existing id in passed ids");
            }

            await Task.Run(() => pendingUploadsToRemove.ForEach( fileId =>
            {
                _filesStorageProvider.Remove(fileId, user.PendingUploadPhotosFolderName);
                _photoService.RemovePhoto(user.Id, fileId);
            }));

            return NoContent();
        }

        private async Task<(Stream stream, IValueProvider formValueProvider)> GetFileStream(Stream body)
        {
            var formAccumulator = new KeyValueAccumulator();

            var boundary = Helpers.MultipartRequestHelper.GetBoundary(
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
                    if (Helpers.MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        fileStream = new MemoryStream();
                        await section.Body.CopyToAsync(fileStream);
                    }
                    else if (Helpers.MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
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

            fileStream.ResetPosition();

            var formValueProvider = new FormValueProvider(
                BindingSource.Form,
                new FormCollection(formAccumulator.GetResults()), CultureInfo.CurrentCulture);

            return (fileStream, formValueProvider);
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
            // most cases.
            return !hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding) ? Encoding.UTF8 : mediaType.Encoding;
        }

        private UserForDetailedDto GetUser()
        {
            var userEmail = User.GetEmail();
            return _userService.GetUser(userEmail);
        }
    }
}
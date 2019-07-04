using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PictureApp.API.Dtos.ModelBinders;

namespace PictureApp.API.Dtos.PhotosDto
{
    [ModelBinder(BinderType = typeof(FileModelBinder))]
    public class PhotosForUploadDto
    {
        public ICollection<IFormFile> Files { get; set; }

        public PhotoForUploadMetadataDto[] Metadata { get; set; }
    }
}

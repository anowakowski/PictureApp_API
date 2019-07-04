using System;

namespace PictureApp.API.Dtos.PhotosDto
{
    public class PhotoForUserDto
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public Uri Url { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
    }
}

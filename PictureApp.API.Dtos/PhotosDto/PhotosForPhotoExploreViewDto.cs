namespace PictureApp.API.Dtos.PhotosDto
{
    public class PhotosForPhotoExploreViewDto
    {
        public bool IsMain { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
    }
}
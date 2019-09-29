namespace PictureApp.API.Dtos.PhotosDto
{
    public class PhotoForUploadMetadataDto
    {
        public string FileName { get; set; }

        public string FileExtension { get; set; }

        public string FileId { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }
        
        public string Description { get; set; }
    }
}

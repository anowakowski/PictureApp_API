using System;
using System.Collections.Generic;

namespace PictureApp.API.Models
{
    public class Photo : Entity
    {
        public string FileId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
        public string PublicId { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }  
        public ICollection<PhotoComment> PhotoComments { get; set; }
    }
}
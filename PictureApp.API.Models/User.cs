using System.Collections.Generic;

namespace PictureApp.API.Models
{
    public class User : Entity
    {        
        public string Username { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public AccountActivationToken ActivationToken { get; set; }
        public bool IsAccountActivated { get; set; }
        public ICollection<Photo> Photos { get; set; }
        public ICollection<UserFollower> Followers { get; set; }
        public ICollection<UserFollower> Following { get; set; }
        public string PendingUploadPhotosFolderName { get; set; }
    }
}
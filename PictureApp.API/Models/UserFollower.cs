namespace PictureApp.API.Models
{
    public class UserFollower : Entity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int FollowerId { get; set; }
        public virtual User Follower { get; set; }
    }
}
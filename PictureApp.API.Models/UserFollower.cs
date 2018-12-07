namespace PictureApp.API.Models
{
    public class UserFollower : Entity
    {
        public int FollowerId { get; set; }
        public virtual User Follower { get; set; }
        public int FolloweeId { get; set; }
        public virtual User Followee { get; set; }
    }
}
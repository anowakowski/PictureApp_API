
namespace PictureApp.API.Dtos
{
    public class UsersListWithFollowersForExploreDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public bool IsFollowerForCurrentUser { get; set; }
        public string Email { get; set; }
        public string PhotoUrl { get; set; }

    }
}
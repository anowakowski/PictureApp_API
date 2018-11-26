using System.Security.Authentication;
using System.Threading.Tasks;
using PictureApp.API.Data;
using PictureApp.API.Data.Repository;
using PictureApp.API.Models;
using PictureApp_API.Services;

namespace PictureApp.API.Services
{
    public class FollowerService : IFollowerService
    {
        private readonly IUserService _userService;
        private readonly IRepository<UserFollower> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public FollowerService(IUserService userService, IRepository<UserFollower> repository, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task SetUpFollower(int userId, int recipientId)
        {
            var user = await _userService.GetUser(userId);
            var recpitent = await _userService.GetUser(recipientId);

            if (user == null)
                throw new AuthenticationException($"user by {userId} not found");
            if (recpitent == null)
                throw new AuthenticationException($"recpitent by id {recipientId} not found");

            var userFollower = await _repository.FirstOrDefaultAsync(
                u => u.FollowerId == userId && u.FolloweeId == recipientId);

            if (userFollower != null)
                throw new System.Exception($"you arleady followed this user");

            var follower = new UserFollower
            {
                FollowerId = userId,
                FolloweeId = recipientId
            };

            await _repository.AddAsync(userFollower);

            await _unitOfWork.CompleteAsync();
        }
    }
}
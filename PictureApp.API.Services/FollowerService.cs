using System;
using System.Linq.Expressions;
using System.Security.Authentication;
using System.Threading.Tasks;
using PictureApp.API.Data;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;
using PictureApp.API.Services;

namespace PictureApp.API.Services
{
    public class FollowerService : IFollowerService
    {
        private readonly IUserService _userService;
        private readonly IRepository<UserFollower> _repository;
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FollowerService(IUserService userService, IRepository<UserFollower> repository, IRepository<User> userRepository, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _repository = repository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UsersListForDiscoverDto> GetUnFollowedUsers(int userId)
        {   
            var user = await _userRepository.SingleAsync(u => u.Id == userId);

            Expression<Func<User, object>>[] includedEntites = {x => x.Followers, x => x.Following};
            var test = await _userRepository.FindAsyncWithIncludedEntities(includedEntites, u => u.Id == userId);
        
            return new UsersListForDiscoverDto();
        }

        public async Task SetUpFollower(int userId, int recipientId)
        {
            var user = await _userService.GetUser(userId);
            var recpitent = await _userService.GetUser(recipientId);

            if (user == null)
                throw new NotAuthorizedException($"user by {userId} not found");
            if (recpitent == null)
                throw new EntityNotFoundException($"recpitent by id {recipientId} not found");

            if (!await _repository.AnyAsync(
                u => u.FollowerId == userId && u.FolloweeId == recipientId))
            {
                var follower = new UserFollower
                {
                    FollowerId = userId,
                    FolloweeId = recipientId
                };

                await _repository.AddAsync(follower);
                await _unitOfWork.CompleteAsync();
            }
            else 
            {
                ////log if user arleady follow choosen recipient
            }
        }
    }
}
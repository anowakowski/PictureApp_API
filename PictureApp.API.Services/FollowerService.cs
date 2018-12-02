using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Authentication;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IRepository<UserFollower> _userFollowerRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FollowerService(
            IUserService userService,
            IRepository<UserFollower> userFollowerRepository, 
            IRepository<User> userRepository, 
            IUnitOfWork unitOfWork, 
            IMapper mapper)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userFollowerRepository = userFollowerRepository ?? throw new ArgumentNullException(nameof(userFollowerRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<UsersListWithFollowersForExploreDto>> GetAllWithFollowers(int userId)
        {   
            var usersFollower = await _userFollowerRepository.FindAsync(x => x.FollowerId == userId);
            var users = await _userRepository.GetAllAsync();

            var usersWithFollowersToReturn = users.Select(user => 
                {
                    var mappedUser = _mapper.Map<UsersListWithFollowersForExploreDto>(user);
                    
                    mappedUser.IsFollowerForCurrentUser = usersFollower.Any(userFollower => userFollower.FolloweeId == user.Id);
     
                    return mappedUser;
                }).ToList();
            
            return usersWithFollowersToReturn;
        }

        public async Task SetUpFollower(int userId, int recipientId)
        {
            var findedUser = _userService.GetUser(userId);
            var findedFollower = _userService.GetUser(recipientId);

            if (findedUser != null && 
                findedUser != null && 
                !await _userFollowerRepository.AnyAsync(
                    u => u.FollowerId == userId && u.FolloweeId == recipientId))
            {
                var follower = new UserFollower
                {
                    FollowerId = userId,
                    FolloweeId = recipientId
                };

                await _userFollowerRepository.AddAsync(follower);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task SetUpUnfollower(int userId, int recipientId)
        {
            var findedUser = _userService.GetUser(userId);
            var findedFollower = _userService.GetUser(recipientId);

            var userFollower = await _userFollowerRepository.FirstOrDefaultAsync(
                u => u.FollowerId == userId && u.FolloweeId == recipientId);

            if (findedUser != null && findedFollower != null && userFollower != null)
            {
                _userFollowerRepository.Delete(userFollower);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
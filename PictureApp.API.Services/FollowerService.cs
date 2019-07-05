using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using PictureApp.API.Data;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Models;

namespace PictureApp.API.Services
{
    public class FollowerService : IFollowerService
    {
        private readonly IUserService _userService;
        private readonly IRepository<UserFollower> _userFollowerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FollowerService(
            IUserService userService,
            IRepository<UserFollower> userFollowerRepository,
            IUnitOfWork unitOfWork, 
            IMapper mapper)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userFollowerRepository = userFollowerRepository ?? throw new ArgumentNullException(nameof(userFollowerRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task SetUpFollower(int userId, int recipientId)
        {
            var findedUser = _userService.GetUser(userId, user => _mapper.Map<UserForDetailedDto>(user));
            var findedFollower = _userService.GetUser(recipientId, user => _mapper.Map<UserForDetailedDto>(user));

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
            var findedUser = _userService.GetUser(userId, user => _mapper.Map<UserForDetailedDto>(user));
            var findedFollower = _userService.GetUser(recipientId, user => _mapper.Map<UserForDetailedDto>(user));

            var userFollower = await _userFollowerRepository.FirstOrDefaultAsync(
                u => u.FollowerId == userId && u.FolloweeId == recipientId);

            if (findedUser != null && findedFollower != null && userFollower != null)
            {
                _userFollowerRepository.Delete(userFollower);
                await _unitOfWork.CompleteAsync();
            }
        }
        public async Task<IEnumerable<UserFollower>> GetFollowers(int userId)
        {
            return await _userFollowerRepository.FindAsync(x => x.FollowerId == userId);
        }
    }
}
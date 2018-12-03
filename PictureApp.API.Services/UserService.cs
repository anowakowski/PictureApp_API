using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;

namespace PictureApp.API.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IMapper _mapper;
        private readonly IFollowerService _followerService;

        public UserService(IRepository<User> userRepo, IMapper mapper, IFollowerService followerService)
        {
            this._followerService = followerService;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userRepository = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        public UserForDetailedDto GetUser(int userId)
        {
            var user = _userRepository.Find(u => u.Id == userId).FirstOrDefault();

            if (user == null)
                throw new EntityNotFoundException($"user by id {userId} not found");

            return _mapper.Map<UserForDetailedDto>(user);
        }

        public async Task<IEnumerable<UsersListWithFollowersForExploreDto>> GetAllWithFollowers(int userId)
        {
            var usersFollower = await _followerService.GetFollowers(userId);
            var users = await _userRepository.GetAllAsync();

            var usersWithFollowersToReturn = users.Select(user =>
                {
                    var mappedUser = _mapper.Map<UsersListWithFollowersForExploreDto>(user);

                    mappedUser.IsFollowerForCurrentUser = usersFollower.Any(userFollower => userFollower.FolloweeId == user.Id);

                    return mappedUser;
                }).ToList();

            return usersWithFollowersToReturn;
        }
    }
}
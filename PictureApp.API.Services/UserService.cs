using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public UserService(IRepository<User> userRepo, IMapper mapper)
        {
            _userRepository = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public UserForDetailedDto GetUser(int userId)
        {
            var user = _userRepository.Find(u => u.Id == userId).FirstOrDefault();

            if (user == null)
                throw new EntityNotFoundException($"user by id {userId} not found");

            return _mapper.Map<UserForDetailedDto>(user);
        }

        public async Task<IEnumerable<UsersListWithFollowersForExploreDto>> GetAllWithFollowers(int currentUserId)
        {
            var usersWithoutCurrentUser = await _userRepository.FindAsyncWithIncludedEntities(u => u.Id != currentUserId,
                include => include.Followers, include => include.Following);

            return usersWithoutCurrentUser.Select(user => _mapper.Map<UsersListWithFollowersForExploreDto>(user)).ToList();
        }
    }
}
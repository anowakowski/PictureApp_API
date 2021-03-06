using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos;
using PictureApp.API.Dtos.PhotosDto;
using PictureApp.API.Dtos.UserDto;
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

        public async Task<T> GetUser<T>(int userId, Func<User, T> func) where T : class
        {
            var users = await _userRepository.FindAsyncWithIncludedEntities(x => x.Id == userId, include => include.Photos);
            var user = users.FirstOrDefault();

            if (user == null)
            {
                throw new EntityNotFoundException($"user with id {userId} not found");
            }
            return func(user);
        }        

        public UserForDetailedDto GetUser(string email)
        {
            var user = _userRepository.Find(u => u.Email == email.ToLower()).FirstOrDefault();

            if (user == null)
            {
                throw new EntityNotFoundException($"User identifies by email {email} does not exist in data store");
            }

            return _mapper.Map<UserForDetailedDto>(user);
        }

        public async Task<UserForDetailedDto> GetUser(int id)
        {
            var user = await _userRepository.GetById(id);

            if (user == null)
            {
                throw new EntityNotFoundException($"User identifies by id {id} does not exist in data store");
            }

            return _mapper.Map<UserForDetailedDto>(user);
        }

        public async Task<IEnumerable<UsersListWithFollowersForExploreDto>> GetAllWithFollowers(int currentUserId)
        {
            var usersWithoutCurrentUser = await _userRepository.FindAsyncWithIncludedEntities(u => u.Id != currentUserId,
                include => include.Followers, include => include.Following, include => include.Photos);

            return usersWithoutCurrentUser.Select(user => _mapper.Map<UsersListWithFollowersForExploreDto>(user)).ToList();
        }
    }
}
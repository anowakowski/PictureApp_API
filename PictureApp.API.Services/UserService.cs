using System;
using System.Threading.Tasks;
using AutoMapper;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos;
using PictureApp.API.Models;

namespace PictureApp.API.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IMapper _mapper;

        public UserService(IRepository<User> userRepo, IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _repository = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        public async Task<UserForDetailedDto> GetUser(int userId)
        {
            var user = await _repository.GetById(userId);
            return _mapper.Map<UserForDetailedDto>(user);
        }
    }
}
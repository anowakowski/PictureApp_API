using System;
using System.Linq;
using AutoMapper;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos;
using PictureApp.API.Extensions.Exceptions;
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

        public UserForDetailedDto GetUser(int userId)
        {
            var user = _repository.Find(u => u.Id == userId).FirstOrDefault();

            if (user == null)
            {
                throw new EntityNotFoundException($"user by id {userId} not found");
            }

            return _mapper.Map<UserForDetailedDto>(user);
        }

        public UserForDetailedDto GetUser(string email)
        {
            var user = _repository.Find(u => u.Email == email.ToLower()).FirstOrDefault();

            if (user == null)
            {
                throw new EntityNotFoundException($"User identifies by email {email} does not exist in data store");
            }

            return _mapper.Map<UserForDetailedDto>(user);
        }
    }
}
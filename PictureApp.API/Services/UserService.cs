using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PictureApp.API.Data;
using PictureApp.API.Data.Repository;
using PictureApp.API.Dtos;
using PictureApp.API.Models;

namespace PictureApp_API.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> userRepo;
        private readonly IMapper _mapper;
        public UserService(IRepository<User> userRepo, IMapper mapper)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }
        public async Task<UserForDetailedDto> GetUser(int userId)
        {
            var findedUser = await userRepo.GetById(userId);
            return _mapper.Map<UserForDetailedDto>(findedUser);
        }

        public async Task<IEnumerable<UserForExploreDto>> GetUsersWithPhotos()
        {
            var returnedUsers = await userRepo.GetAllWithIncludedEntities(c => c.Photos);

            return _mapper.Map<IEnumerable<UserForExploreDto>>(returnedUsers);
        }
    }
}
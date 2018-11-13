using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PictureApp.API.Data;
using PictureApp.API.Data.Repository;
using PictureApp.API.Models;
using PictureApp_API.Dtos;

namespace PictureApp_API.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> userRepo;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        public UserService(IRepository<User> userRepo, IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.userRepo = userRepo;
        }

        public async Task<UserForDetailedDto> GetUser(int userId)
        {
            var findedUser = await userRepo.GetById(userId);

            return mapper.Map<UserForDetailedDto>(findedUser);
        }
    }
}
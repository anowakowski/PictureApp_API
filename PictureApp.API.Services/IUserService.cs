using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PictureApp.API.Dtos;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Models;

namespace PictureApp.API.Services
{
    public interface IUserService
    {
        Task<T> GetUser<T>(int userId, Func<User, T> func) where T : class;
        UserForDetailedDto GetUser(string email);
        Task<IEnumerable<UsersListWithFollowersForExploreDto>> GetAllWithFollowers(int userId);
    }
}
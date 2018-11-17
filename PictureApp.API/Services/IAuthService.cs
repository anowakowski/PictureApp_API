﻿using System.Threading.Tasks;
using PictureApp.API.Dtos;
using PictureApp.API.Models;

namespace PictureApp.API.Services
{
    public interface IAuthService
    {
        Task Register(UserForRegisterDto userForRegister);
        Task<UserLoggedInDto> Login(string email, string password);
        Task<bool> UserExists(string email);
    }
}

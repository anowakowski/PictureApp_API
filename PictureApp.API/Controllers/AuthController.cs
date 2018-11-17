using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PictureApp.API.Data.Repository;
using PictureApp.API.Dtos;
using PictureApp.API.Exceptions;
using PictureApp.API.Models;
using PictureApp.API.Providers;
using PictureApp.API.Services;

namespace PictureApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepository;
        private readonly IAuthTokenProvider _jwtToken;
        private readonly IAuthService _authService;

        public AuthController(IAuthRepository authRepository, IAuthTokenProvider jwtToken, IAuthService authService)
        {
            this.authRepository = authRepository;
            _jwtToken = jwtToken;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegister)
        {
            userForRegister.Username = userForRegister.Username.ToLower();

            if (await authRepository.UserExists(userForRegister.Username))
            {
                return BadRequest("User name already exists");
            }

            var userToCreate = new User
            {
                Username = userForRegister.Username,
                Email = userForRegister.Email

            };

            var createdUser = await authRepository.Register(userToCreate, userForRegister.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserForLoginDto userForLogin)
        {
            var userFromRepo = await _authService.Login(userForLogin.Email.ToLower(), userForLogin.Password);

            if (userFromRepo == null)
                return Unauthorized();

            var token = _jwtToken.GetToken(
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Email, userFromRepo.Email));                

            return Ok(new
            {
                token = token
            });

        }
 
    }
}
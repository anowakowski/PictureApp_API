using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PictureApp.API.Dtos;
using PictureApp.API.Providers;
using PictureApp.API.Services;
using PictureApp.API.Extensions.Exceptions;

namespace PictureApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthTokenProvider _jwtToken;
        private readonly IAuthService _authService;

        public AuthController(IAuthTokenProvider jwtToken, IAuthService authService)
        {
            _jwtToken = jwtToken ?? throw new ArgumentNullException(nameof(jwtToken));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegister)
        {
            userForRegister.Email = userForRegister.Email.ToLower();

            if (await _authService.UserExists(userForRegister.Email))
            {
                return BadRequest("User name already exists");
            }

            await Task.Run(() => _authService.Register(userForRegister));
            
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserForLoginDto userForLogin)
        {
            try 
            {
                var userForLoggedDto = await _authService.Login(userForLogin.Email.ToLower(), userForLogin.Password);

                if (userForLoggedDto == null)
                    return Unauthorized();

                var token = _jwtToken.GetToken(
                    new Claim(ClaimTypes.NameIdentifier, userForLoggedDto.Id.ToString()),
                    new Claim(ClaimTypes.Email, userForLoggedDto.Email),
                    new Claim(ClaimTypes.Name, userForLoggedDto.Username));                

                return Ok(new
                {
                    token = token
                });                

            } catch(EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
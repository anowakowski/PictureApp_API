using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PictureApp.API.Dtos;
using PictureApp.API.Exceptions;
using PictureApp.API.Providers;
using PictureApp.API.Services;

namespace PictureApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;        
        private readonly IAuthTokenProvider _tokenProvider;

        public AuthController(IAuthService authService, IAuthTokenProvider tokenProvider)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegister)
        {
            userForRegister.Username = userForRegister.Username.ToLower();

            if (await _authService.UserExists(userForRegister.Email.ToLower()))
            {
                return BadRequest("User email already exists");
            }

            _authService.Register(userForRegister);
            
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("login")]
        public ActionResult Login(UserForLoginDto userForLogin)
        {
            try
            {
                _authService.Login(userForLogin.Email, userForLogin.Password);
                var loggedInUser = _authService.GetLoggedInUser(userForLogin.Email.ToLower());

                var token = _tokenProvider.GetToken(
                    new Claim(ClaimTypes.NameIdentifier, loggedInUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, loggedInUser.Email));

                return Ok(new { token });
            }
            catch (Exception e) when (e is EntityNotFoundException || e is NotAuthorizedException)
            {
                return Unauthorized();
            }
        }
    }
}
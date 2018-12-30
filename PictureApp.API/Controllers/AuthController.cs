using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PictureApp.API.Dtos;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Providers;
using PictureApp.API.Services;
using PictureApp.Messaging;

namespace PictureApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthTokenProvider _authTokenProvider;
        private readonly IAuthService _authService;
        private readonly IMediator _mediator;
        
        public AuthController(IAuthTokenProvider authTokenProvider, IAuthService authService, IMediator mediator)
        {
            _authTokenProvider = authTokenProvider ?? throw new ArgumentNullException(nameof(authTokenProvider));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
            
            await _mediator.Publish(new UserRegisteredNotificationEvent(userForRegister.Email));
            
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("activate")]
        public async Task<IActionResult> Activate(UserForRegisterActivateDto userForRegisterActivate)
        {
            try
            {
                await _authService.Activate(userForRegisterActivate.Token);
            }
            catch (Exception ex)
            {
                if (ex is SecurityTokenExpiredException || ex is EntityNotFoundException)
                {
                    return BadRequest("User can not be activated: token expired or does not exist");
                }

                throw;
            }
                        
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserForLoginDto userForLogin)
        {
            try 
            {
                var userForLoggedDto = await _authService.Login(userForLogin.Email.ToLower(), userForLogin.Password);

                if (userForLoggedDto == null)
                    return Unauthorized();

            var token = _authTokenProvider.GetToken(
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
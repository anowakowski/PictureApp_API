using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PictureApp.API.Dtos;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Extensions;
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

            return NoContent();
        }

        [HttpPost("reregister")]
        public async Task<IActionResult> Reregister(UserForReregisterDto userForReregister)
        {
            try
            {
                await Task.Run(() => _authService.Reregister(userForReregister));
            }
            catch (Exception ex)
            {
                if (ex is EntityNotFoundException || ex is NotAuthorizedException)
                {
                    return BadRequest("User does not exist or his account is already activated");
                }

                throw;
            }

            await _mediator.Publish(new UserRegisteredNotificationEvent(userForReregister.Email));

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("activate")]
        public async Task<IActionResult> Activate(UserForRegisterActivateDto userForRegisterActivate)
        {
            try
            {
                await _authService.Activate(userForRegisterActivate.Token);
            }
            catch (Exception ex) when (ex is SecurityTokenExpiredException || ex is EntityNotFoundException)
            {
                return BadRequest("User can not be activated: token expired or does not exist");
            }

            return NoContent();
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(UserForChangePasswordDto userForChangePassword)
        {
            try
            {
                await _authService.ChangePassword(User.GetEmail(), userForChangePassword.OldPassword,
                    userForChangePassword.NewPassword,
                    userForChangePassword.RetypedPassword);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    $"Wrong {nameof(userForChangePassword.OldPassword)} or given {nameof(userForChangePassword.NewPassword)} is not the same as {nameof(userForChangePassword.RetypedPassword)}");
            }

            return NoContent();
        }

        [HttpPost("ResetPasswordRequest")]
        public async Task<IActionResult> ResetPasswordRequest(ResetPasswordRequestDto resetPasswordRequestDto)
        {
            try
            {
                await _authService.ResetPasswordRequest(resetPasswordRequestDto.Email);
                await _mediator.Publish(new ResetPasswordRequestNotificationEvent(resetPasswordRequestDto.Email));
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            
            return NoContent();
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(UserResetPasswordDto resetPasswordDto)
        {
            try
            {
                await _authService.ResetPassword(resetPasswordDto.Token, resetPasswordDto.Password);
            }
            catch (Exception ex) when(ex is SecurityTokenExpiredException || ex is EntityNotFoundException)
            {
                return BadRequest("The new password can not be provided: token expired or does not exist");
            }

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLogin)
        {
            try
            {
                var userForLoggedDto = await _authService.Login(userForLogin.Email.ToLower(), userForLogin.Password);

                if (userForLoggedDto == null)
                    return Unauthorized();

                var token = _authTokenProvider.GetToken(
                    new Claim(ClaimTypes.NameIdentifier, userForLoggedDto.Id.ToString()),
                    new Claim(ClaimTypes.Email, userForLoggedDto.Email),
                    new Claim(ClaimTypes.Name, userForLoggedDto.Username)
                );

                return Ok(new
                {
                    token
                });
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (NotAuthorizedException ex)
            {
                return Unauthorized();
            }
        }        
    }
}
using System;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Services;

namespace PictureApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/followers")]
    [ApiController]
    public class FollowersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IFollowerService _followerService;

        public FollowersController(IUserService userService, IFollowerService followerService)
        {
            _userService = userService;
            _followerService = followerService;
        }

        [HttpPost("{id}/setfollow")]
        public async Task<IActionResult> SetUpFollower(int userId, int id)
        {
            if (AuthorizeUser(userId))
                return Unauthorized();

            try
            {
                await Task.Run(() => _followerService.SetUpFollower(userId, id));

                return Ok();
            }
            catch (EntityNotFoundException)
            {
                return BadRequest("Can't set up follower");
            }
        }

        [HttpPost("{id}/setunfollow")]
        public async Task<IActionResult> SetUpUnFollow(int userId, int id)
        {
            if (AuthorizeUser(userId))
                return Unauthorized();

            try 
            {
                await Task.Run(() => _followerService.SetUpUnfollower(userId, id));

                return Ok();
            }
            catch (EntityNotFoundException)
            {
                return BadRequest("Can't unfollow that user");
            }            
        }

        [HttpGet("allUserWithFollowerInfo")]
        public async Task<IActionResult> GetUnFollowedUsers(int userId)
        {
            return Ok(await _userService.GetAllWithFollowers(userId));
        }

        private bool AuthorizeUser(int userId)
        {
            if (userId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return true;

            return false;
        }
    }
}
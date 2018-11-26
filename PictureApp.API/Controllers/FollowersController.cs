using System;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PictureApp.API.Services;
using PictureApp_API.Services;

namespace PictureApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/followers")]
    [ApiController]
    public class FollowersController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IFollowerService _followerService;

        public FollowersController(IUserService userService, IFollowerService followerService)
        {
            this.userService = userService;
            _followerService = followerService;
        }

        [HttpPost]
        public async Task<IActionResult> SetUpFollower(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            try
            {
                await Task.Run(() => _followerService.SetUpFollower(userId, id));

                return Ok();
            }
            catch (AuthenticationException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
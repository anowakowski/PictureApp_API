using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PictureApp.API.Services;

namespace PictureApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserService userService, IMapper mapper, IPhotoService photoService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _photoService = photoService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            return Ok(await _userService.GetUser(id));
        }

        [HttpGet("userEditProfile/{id}")]
        public async Task<IActionResult> GetUserForEdit(int id)
        {
            return Ok(await _userService.GetUserForEdit(id));
        }

        [HttpGet("getCurrentUserFollowersForDashboard/{id}")]
        public async Task<IActionResult> GetUsersWithFollowersForCurrentUser(int id)
        {
            try
            {
                CheckIfCurrentUserIsCorrect(id);

                var users = await _userService.GetAllWithFollowers(id);
                await Task.Run(() => _photoService.GetUsersPhotosWithComments(users));

                return Ok(users);
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        private void CheckIfCurrentUserIsCorrect(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (currentUserId != id)
            {
                throw new Exception("requested user Id is different by current user id");
            }
        }
    }
}
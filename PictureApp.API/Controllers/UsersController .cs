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

        [HttpGet("allUserWithFollowerInfo")]
        public async Task<IActionResult> GetUsersWithFollowers()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var users = await _userService.GetAllWithFollowers(currentUserId);
            await Task.Run(() => _photoService.SetUserPhotoWithComments(users));

            return Ok(users);
        }     
    }
}
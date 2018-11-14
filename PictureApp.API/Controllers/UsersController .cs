using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PictureApp_API.Services;
 namespace PictureApp_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
     public class UsersController : ControllerBase
    {
        private readonly IUserService userService;
        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }   
         [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            return Ok(await userService.GetUser(id));
        }
    }
} 
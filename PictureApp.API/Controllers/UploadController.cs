using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Models;
using PictureApp.API.Services;

namespace PictureApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IRepository<User> _userRepository;

        public UploadController(IFileUploadService fileUploadService, IRepository<User> userRepository)
        {
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }
        
        [HttpPost("upload"), DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFile()
        {
            var userEmail = User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var user = await _userRepository.SingleOrDefaultAsync(x => x.Email == userEmail);

            Request.Form.Files.ForEach(x =>
            {
                Stream stream = new MemoryStream();
                x.CopyToAsync(stream);
                _fileUploadService.Upload(stream, user.Id);
            });

            return StatusCode(StatusCodes.Status201Created);
        }
    }
}
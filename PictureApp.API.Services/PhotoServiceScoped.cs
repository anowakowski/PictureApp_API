using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PictureApp.API.Dtos.PhotosDto;
using PictureApp.API.Dtos.UserDto;

namespace PictureApp.API.Services
{
    public class PhotoServiceScoped : IPhotoServiceScoped
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Func<IServiceScope, IPhotoService> _photoServiceFactory;

        public PhotoServiceScoped(IServiceScopeFactory serviceScopeFactory, Func<IServiceScope, IPhotoService> photoServiceFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _photoServiceFactory = photoServiceFactory ?? throw new ArgumentNullException(nameof(photoServiceFactory));
        }

        public async Task SetUsersPhotosWithComments(IEnumerable<UsersListWithFollowersForExploreDto> users)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var photoService = _photoServiceFactory(scope);
                await photoService.SetUsersPhotosWithComments(users);
            }
        }

        public async Task AddPhotoForUser(PhotoForUserDto photoForUser)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var photoService = _photoServiceFactory(scope);
                await photoService.AddPhotoForUser(photoForUser);
            }
        }

        public async Task UpdatePhotoForUser(PhotoForUserDto photoForUser)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var photoService = _photoServiceFactory(scope);
                await photoService.UpdatePhotoForUser(photoForUser);
            }
        }

        public async Task RemovePhoto(int userId, string fileId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var photoService = _photoServiceFactory(scope);
                await photoService.RemovePhoto(userId, fileId);
            }
        }
    }
}
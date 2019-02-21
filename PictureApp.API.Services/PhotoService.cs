using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos.PhotosDto;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Models;

namespace PictureApp.API.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IRepository<Photo> _repo;
        private readonly IMapper _mapper;

        public PhotoService(IRepository<Photo> repo, IMapper mapper)
        {
            _repo = repo;
            this._mapper = mapper;
        }
        public async Task SetUserPhotoWithComments(IEnumerable<UsersListWithFollowersForExploreDto> users)
        {
            foreach(var user in users)
            {
                await SetUserPhotos(user);
            }
        }

        private async Task SetUserPhotos(UsersListWithFollowersForExploreDto user)
        {
            var userPhotos = await _repo.FindAsyncWithIncludedEntities(x => x.UserId == user.Id, include => include.PhotoComments);
            user.Photos = _mapper.Map<IEnumerable<PhotosForPhotoExploreViewDto>>(userPhotos);
        }
    }
}
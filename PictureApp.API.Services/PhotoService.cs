using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PictureApp.API.Data;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos.PhotosDto;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;

namespace PictureApp.API.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IRepository<Photo> _repository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public PhotoService(IRepository<Photo> repository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task SetUsersPhotosWithComments(IEnumerable<UsersListWithFollowersForExploreDto> users)
        {
            foreach(var user in users)
            {
                await SetUserPhotos(user);
            }
        }

        public async Task AddPhotoForUser(PhotoForUserDto photoForUser)
        {
            var photo = _mapper.Map<Photo>(photoForUser);

            await _repository.AddAsync(photo);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdatePhotoForUser(PhotoForUserDto photoForUser)
        {
            var photo = await GetPhoto(photoForUser.UserId, photoForUser.FileId);            
            photo = _mapper.Map(photoForUser, photo);

            _repository.Update(photo);
            await _unitOfWork.CompleteAsync();
        }

        public async Task RemovePhoto(int userId, string fileId)
        {
            var photo = await GetPhoto(userId, fileId);           
            _repository.Delete(photo);
            await _unitOfWork.CompleteAsync();
        }

        private async Task SetUserPhotos(UsersListWithFollowersForExploreDto user)
        {
            var userPhotos = await _repository.FindAsyncWithIncludedEntities(x => x.UserId == user.Id, include => include.PhotoComments);
            user.Photos = _mapper.Map<IEnumerable<PhotosForPhotoExploreViewDto>>(userPhotos);
        }

        private async Task<Photo> GetPhoto(int userId, string fileId)
        {
            var photos = await _repository.FindAsync(x => x.UserId == userId && x.PublicId == fileId);
            var photo = photos.SingleOrDefault();
            if (photo == null)
            {
                throw new EntityNotFoundException($"There is no photo with given fileId: {fileId} and userId: {userId}");
            }

            return photo;
        }
    }
}
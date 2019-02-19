using System.Linq;
using AutoMapper;
using PictureApp.API.Dtos;
using PictureApp.API.Dtos.PhotosDto;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Models;

namespace PictureApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            FromDomainToDto();
            FromDtoToDomain();
        }

        private void FromDtoToDomain()
        {
            CreateMap<UserForRegisterDto, User>();
        }

        private void FromDomainToDto()
        {
            CreateMap<User, UserForRegisterDto>()
                .ForMember(dest => dest.Password, opt => opt.Ignore());
            CreateMap<User, UserLoggedInDto>();
            CreateMap<Photo, PhotosForPhotoExploreViewDto>();
            CreateMap<User, UserForDetailedDto>()
                .ForMember(dest => dest.ActivationToken,
                    opt => { opt.MapFrom(src => src.ActivationToken != null ? src.ActivationToken.Token : null); })
                .ForMember(dest => dest.PhotoUrl,
                    opt => {opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url); });    
            CreateMap<User, UsersListWithFollowersForExploreDto>()
                .ForMember(dest => dest.IsFollowerForCurrentUser, opt => {
                    opt.MapFrom(src => src.Following.Any(x => x.FolloweeId == src.Id));
                });   
            CreateMap<User, UserForEditProfileDto>();                           
        }
    }
}
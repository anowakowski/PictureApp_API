using AutoMapper;
using PictureApp.API.Dtos;
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
        }
    }
}
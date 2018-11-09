using AutoMapper;
using PictureApp.API.Models;
using PictureApp_API.Dtos;

namespace PictureApp_API.Helpers
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
        }
    }
}
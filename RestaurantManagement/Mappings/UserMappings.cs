using AutoMapper;
using RestaurantManagement.Dtos.User;
using RestaurantManagement.Entities;

namespace RestaurantManagement.Profiles
{
    public class UserProfiles : Profile
    {
        public UserProfiles()
        {
            CreateMap<RegisterDto, User>();
            CreateMap<UpdateUserDto, User>();
            CreateMap<User, UserResponseDto>();
        }
    }
}
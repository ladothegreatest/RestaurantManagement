using AutoMapper;
using RestaurantManagement.Common.Enums;
using RestaurantManagement.Dtos.Reservation;
using RestaurantManagement.Entities;

namespace RestaurantManagement.Mappings
{
    public class ReservationMappings : Profile
    {
        public ReservationMappings()
        {
            CreateMap<CreateReservationDto, Reservation>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ReservationStatus.Pending))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateReservationDto, Reservation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.TableId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<Reservation, ReservationResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : null))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.TableNumber, opt => opt.MapFrom(src => src.Table != null ? src.Table.TableNumber : 0))
                .ForMember(dest => dest.TableCapacity, opt => opt.MapFrom(src => src.Table != null ? src.Table.Capacity : 0))
                .ForMember(dest => dest.RestaurantId, opt => opt.MapFrom(src => src.Table != null ? src.Table.RestaurantId : 0))
                .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Table != null && src.Table.Restaurant != null ? src.Table.Restaurant.Name : null));
        }
    }
}

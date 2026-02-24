using AutoMapper;
using RestaurantManagement.Dtos.Table;
using RestaurantManagement.Entities;

namespace RestaurantManagement.Mappings
{
    public class TableMappings
    {
        public class TableProfiles : Profile
        {
            public TableProfiles()
            {
                CreateMap<CreateTableDto, RestaurantTable>()
                    .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => true));

                CreateMap<UpdateTableDto, RestaurantTable>();

                CreateMap<RestaurantTable, TableResponseDto>()
                    .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.Restaurant != null ? src.Restaurant.Name : null));
            }
        }
    }
}

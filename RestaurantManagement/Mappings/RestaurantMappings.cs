using AutoMapper;
using RestaurantManagement.Common.Enums;
using RestaurantManagement.Dtos.Restaurant;
using RestaurantManagement.Entities;

namespace RestaurantManagement.Mappings
{
    public class RestaurantMappings : Profile
    {
        public RestaurantMappings()
        {
            CreateMap<CreateRestaurantDto, Restaurant>()
                .ForMember(dest => dest.OperatingDays, opt => opt.MapFrom(src => ConvertToDayOfWeekFlags(src.OperatingDays)));

            CreateMap<UpdateRestaurantDto, Restaurant>()
                .ForMember(dest => dest.OperatingDays, opt => opt.MapFrom(src => ConvertToDayOfWeekFlags(src.OperatingDays)));

            CreateMap<Restaurant, RestaurantResponseDto>()
                .ForMember(dest => dest.OperatingDays, opt => opt.MapFrom(src => ConvertToStringArray(src.OperatingDays)))
                .ForMember(dest => dest.TotalTables, opt => opt.MapFrom(src => src.Tables != null ? src.Tables.Count : 0))
                .ForMember(dest => dest.AvailableTables, opt => opt.MapFrom(src => src.Tables != null ? src.Tables.Count(t => t.IsAvailable) : 0));
        }

        private DayOfWeekFlags ConvertToDayOfWeekFlags(DayOfWeek[]? days)
        {
            if (days == null || days.Length == 0)
                return DayOfWeekFlags.None;

            DayOfWeekFlags result = DayOfWeekFlags.None;
            foreach (var day in days)
            {
                result |= (DayOfWeekFlags)(1 << (int)day);
            }
            return result;
        }

        private string[] ConvertToStringArray(DayOfWeekFlags flags)
        {
            var days = new List<string>();

            if (flags.HasFlag(DayOfWeekFlags.Monday)) days.Add("Monday");
            if (flags.HasFlag(DayOfWeekFlags.Tuesday)) days.Add("Tuesday");
            if (flags.HasFlag(DayOfWeekFlags.Wednesday)) days.Add("Wednesday");
            if (flags.HasFlag(DayOfWeekFlags.Thursday)) days.Add("Thursday");
            if (flags.HasFlag(DayOfWeekFlags.Friday)) days.Add("Friday");
            if (flags.HasFlag(DayOfWeekFlags.Saturday)) days.Add("Saturday");
            if (flags.HasFlag(DayOfWeekFlags.Sunday)) days.Add("Sunday");

            return days.ToArray();
        }
    }
}

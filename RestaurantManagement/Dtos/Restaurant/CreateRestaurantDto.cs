using RestaurantManagement.Common.Validators;
using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Dtos.Restaurant
{
    public class CreateRestaurantDto
    {
        [Required(ErrorMessage = "Restaurant name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, MinimumLength = 5)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        public bool IsOpen { get; set; } = true;

        [Required(ErrorMessage = "Opening time is required")]
        [DataType(DataType.Time)]
        public TimeSpan OpeningTime { get; set; }

        [Required(ErrorMessage = "Closing time is required")]
        [DataType(DataType.Time)]
        [GreaterThan("OpeningTime", ErrorMessage = "Closing time must be after opening time")]
        public TimeSpan ClosingTime { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one operating day must be selected")]
        public DayOfWeek[] OperatingDays { get; set; } = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };

        [Range(15, 240, ErrorMessage = "Table turnover time must be between 15 and 240 minutes")]
        public int TableTurnoverMinutes { get; set; } = 120;
    }
}

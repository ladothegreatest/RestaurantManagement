using RestaurantManagement.Common.Validators;
using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Dtos.Restaurant
{
    public class UpdateRestaurantDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Address { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public bool IsOpen { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan OpeningTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [GreaterThan("OpeningTime")]
        public TimeSpan ClosingTime { get; set; }

        [Required]
        [MinLength(1)]
        public DayOfWeek[] OperatingDays { get; set; }

        [Range(15, 240)]
        public int TableTurnoverMinutes { get; set; }
    }
}
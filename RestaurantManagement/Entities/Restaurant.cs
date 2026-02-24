using RestaurantManagement.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Entities
{
    public class Restaurant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        public bool IsOpen { get; set; }

        [Required]
        public TimeSpan OpeningTime { get; set; }

        [Required]
        public TimeSpan ClosingTime { get; set; }

        public DayOfWeekFlags OperatingDays { get; set; } = DayOfWeekFlags.All;

        [Range(15, 240)]
        public int TableTurnoverMinutes { get; set; } = 120;

        public ICollection<RestaurantTable> Tables { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}

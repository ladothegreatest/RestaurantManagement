using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Dtos.Reservation
{
    public class CreateReservationDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Table ID is required")]
        public int TableId { get; set; }

        [Required(ErrorMessage = "Reservation time is required")]
        [DataType(DataType.DateTime)]
        public DateTime ReservationTime { get; set; }

        [Required(ErrorMessage = "Number of guests is required")]
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        public int NumberOfGuests { get; set; }
    }
}
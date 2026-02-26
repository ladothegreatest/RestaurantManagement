using RestaurantManagement.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagement.Entities
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("Table")]
        public int TableId { get; set; }

        [Required]
        public DateTime ReservationTime { get; set; }

        [Required]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public RestaurantTable? Table { get; set; }

        [NotMapped]
        public Restaurant? Restaurant => Table?.Restaurant;
    }
}

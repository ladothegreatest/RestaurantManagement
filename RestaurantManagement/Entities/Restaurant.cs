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

        public ICollection<RestaurantTable> Tables { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}

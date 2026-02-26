using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(100)]
        public string? Password { get; set; }

        public ICollection<Reservation>? Reservations { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Dtos.Table
{
    public class UpdateTableDto
    {
        [Required]
        [Range(1, 999)]
        public int TableNumber { get; set; }

        [Required]
        [Range(1, 20)]
        public int Capacity { get; set; }

        [Required]
        public bool IsAvailable { get; set; }
    }
}

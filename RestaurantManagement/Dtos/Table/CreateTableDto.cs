using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Dtos.Table
{
    public class CreateTableDto
    {
        [Required(ErrorMessage = "Table number is required")]
        [Range(1, 999, ErrorMessage = "Table number must be between 1 and 999")]
        public int TableNumber { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Restaurant ID is required")]
        public int RestaurantId { get; set; }
    }
}
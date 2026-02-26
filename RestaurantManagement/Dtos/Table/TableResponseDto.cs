namespace RestaurantManagement.Dtos.Table
{
    public class TableResponseDto
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
    }
}
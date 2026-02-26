namespace RestaurantManagement.Dtos.Restaurant
{
    public class RestaurantResponseDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsOpen { get; set; }

        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public string[]? OperatingDays { get; set; }

        public int TableTurnoverMinutes { get; set; }

        public int TotalTables { get; set; }
        public int AvailableTables { get; set; }
    }
}
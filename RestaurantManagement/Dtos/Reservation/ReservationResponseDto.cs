namespace RestaurantManagement.Dtos.Reservation
{
    public class ReservationResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public int TableCapacity { get; set; }


        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }

        public DateTime ReservationTime { get; set; }
        public int NumberOfGuests { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
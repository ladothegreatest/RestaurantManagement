namespace RestaurantManagement.Common
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string>? Errors { get; set; }
    }
}

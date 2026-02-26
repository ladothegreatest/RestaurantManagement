namespace RestaurantManagement.Dtos.User
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public UserResponseDto User { get; set; }
    }
}
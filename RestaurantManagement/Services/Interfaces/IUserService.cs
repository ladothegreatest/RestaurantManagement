using RestaurantManagement.Dtos.User;

namespace RestaurantManagement.Services.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponseDto> RegisterAsync(RegisterDto dto);
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        Task<UserResponseDto?> GetByIdAsync(int id);
        Task<UserResponseDto?> GetByEmailAsync(string email);
        Task<List<UserResponseDto>> GetAllAsync();
        Task<UserResponseDto?> UpdateAsync(int id, UpdateUserDto dto);
        Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

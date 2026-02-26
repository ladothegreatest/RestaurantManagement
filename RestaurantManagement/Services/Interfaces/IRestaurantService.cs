using RestaurantManagement.Dtos.Restaurant;

namespace RestaurantManagement.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<RestaurantResponseDto> CreateAsync(CreateRestaurantDto createRestaurantDto);
        Task<RestaurantResponseDto?> GetByIdAsync(int id);
        Task<List<RestaurantResponseDto>> GetAllAsync();
        Task<List<RestaurantResponseDto>> GetOpenRestaurantsAsync();
        Task<List<RestaurantResponseDto>> GetByNameAsync(string name);
        Task<RestaurantResponseDto?> UpdateAsync(int id, UpdateRestaurantDto dto);
        Task<bool> ToggleOpenStatusAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}

using RestaurantManagement.Dtos.Table;

namespace RestaurantManagement.Services.Interfaces
{
    public interface ITableService
    {
        Task<TableResponseDto> CreateAsync(CreateTableDto dto);
        Task<TableResponseDto?> GetByIdAsync(int id);
        Task<List<TableResponseDto>> GetAllAsync();
        Task<List<TableResponseDto>> GetByRestaurantIdAsync(int restaurantId);
        Task<List<TableResponseDto>> GetAvailableTablesByRestaurantIdAsync(int restaurantId);
        Task<List<TableResponseDto>> GetByCapacityAsync(int restaurantId, int minCapacity);
        Task<TableResponseDto?> UpdateAsync(int id, UpdateTableDto dto);
        Task<bool> ToggleAvailabilityAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}

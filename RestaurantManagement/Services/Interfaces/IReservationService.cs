using RestaurantManagement.Common.Enums;
using RestaurantManagement.Common;
using RestaurantManagement.Dtos.Reservation;

namespace RestaurantManagement.Services.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationResponseDto> CreateAsync(CreateReservationDto dto);
        Task<ReservationResponseDto?> GetByIdAsync(int id);
        Task<List<ReservationResponseDto>> GetAllAsync();
        Task<List<ReservationResponseDto>> GetByUserIdAsync(int userId);
        Task<List<ReservationResponseDto>> GetByRestaurantIdAsync(int restaurantId);
        Task<List<ReservationResponseDto>> GetByTableIdAsync(int tableId);
        Task<List<ReservationResponseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<ReservationResponseDto>> GetByStatusAsync(ReservationStatus status);
        Task<ReservationResponseDto?> UpdateAsync(int id, UpdateReservationDto dto);
        Task<bool> UpdateStatusAsync(int id, UpdateReservationStatusDto dto);
        Task<bool> CancelAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<PagedResult<ReservationResponseDto>> GetPagedAsync(int page, int pageSize);
    }
}

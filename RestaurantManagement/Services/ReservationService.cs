using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantManagement.Common;
using RestaurantManagement.Common.Enums;
using RestaurantManagement.Dtos.Reservation;
using RestaurantManagement.Entities;
using RestaurantManagement.Repositories;
using RestaurantManagement.Services.Interfaces;

namespace RestaurantManagement.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Reservation> _reservationRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<RestaurantTable> _tableRepository;
        private readonly IRepository<Restaurant> _restaurantRepository;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IMapper mapper,
            IRepository<Reservation> reservationRepository,
            IRepository<User> userRepository,
            IRepository<RestaurantTable> tableRepository,
            IRepository<Restaurant> restaurantRepository,
            ILogger<ReservationService> logger)
        {
            _mapper = mapper;
            _reservationRepository = reservationRepository;
            _userRepository = userRepository;
            _tableRepository = tableRepository;
            _restaurantRepository = restaurantRepository;
            _logger = logger;
        }

        public async Task<ReservationResponseDto> CreateAsync(CreateReservationDto dto)
        {
            _logger.LogInformation("Creating reservation for user {UserId} at table {TableId}", dto.UserId, dto.TableId);

            if (!await _userRepository.ExistsAsync(dto.UserId))
            {
                _logger.LogWarning("User with ID {UserId} not found", dto.UserId);
                throw new ArgumentException("User not found");
            }

            var table = await _tableRepository.GetByIdWithIncludesAsync(dto.TableId, t => t.Restaurant);
            if (table == null)
            {
                _logger.LogWarning("Table with ID {TableId} not found", dto.TableId);
                throw new ArgumentException("Table not found");
            }

            var restaurant = table.Restaurant;
            if (restaurant == null)
            {
                _logger.LogWarning("Restaurant not found for table {TableId}", dto.TableId);
                throw new ArgumentException("Restaurant not found");
            }

            if (!restaurant.IsOpen)
            {
                _logger.LogWarning("Reservation failed - restaurant {RestaurantId} is currently closed", restaurant.Id);
                throw new InvalidOperationException("Restaurant is currently closed");
            }

            if (dto.NumberOfGuests > table.Capacity)
            {
                _logger.LogWarning("Reservation failed - guests {NumberOfGuests} exceeds table capacity {Capacity}", dto.NumberOfGuests, table.Capacity);
                throw new InvalidOperationException($"Table capacity is {table.Capacity}, but {dto.NumberOfGuests} guests requested");
            }

            var dayOfWeek = dto.ReservationTime.DayOfWeek;
            var dayFlag = (DayOfWeekFlags)(1 << (int)dayOfWeek);
            if (!restaurant.OperatingDays.HasFlag(dayFlag))
            {
                _logger.LogWarning("Reservation failed - restaurant {RestaurantId} is closed on {DayOfWeek}", restaurant.Id, dayOfWeek);
                throw new InvalidOperationException($"Restaurant is closed on {dayOfWeek}s");
            }

            var reservationTimeOnly = dto.ReservationTime.TimeOfDay;
            if (reservationTimeOnly < restaurant.OpeningTime)
            {
                _logger.LogWarning("Reservation failed - time {ReservationTime} is before opening time {OpeningTime}", reservationTimeOnly, restaurant.OpeningTime);
                throw new InvalidOperationException($"Restaurant opens at {restaurant.OpeningTime}. Please choose a later time.");
            }

            if (reservationTimeOnly > restaurant.ClosingTime)
            {
                _logger.LogWarning("Reservation failed - time {ReservationTime} is after closing time {ClosingTime}", reservationTimeOnly, restaurant.ClosingTime);
                throw new InvalidOperationException($"Restaurant closes at {restaurant.ClosingTime}. Please choose an earlier time.");
            }

            var reservationEndTime = dto.ReservationTime.AddMinutes(restaurant.TableTurnoverMinutes);
            if (reservationEndTime.TimeOfDay > restaurant.ClosingTime)
            {
                _logger.LogWarning("Reservation failed - end time {EndTime} exceeds closing time {ClosingTime}", reservationEndTime.TimeOfDay, restaurant.ClosingTime);
                throw new InvalidOperationException($"This reservation would extend past closing time ({restaurant.ClosingTime:hh\\:mm}). Please book earlier.");
            }

            var hasOverlap = await _reservationRepository.AnyAsync(r =>
                r.TableId == dto.TableId &&
                r.Status != ReservationStatus.Cancelled &&
                r.ReservationTime < reservationEndTime &&
                r.ReservationTime.AddMinutes(restaurant.TableTurnoverMinutes) > dto.ReservationTime
            );

            if (hasOverlap)
            {
                _logger.LogWarning("Reservation failed - table {TableId} already reserved at {ReservationTime}", dto.TableId, dto.ReservationTime);
                throw new InvalidOperationException("This table is already reserved for this time slot. Please choose a different time.");
            }

            var reservation = _mapper.Map<Reservation>(dto);

            await _reservationRepository.AddAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            var created = await _reservationRepository.GetByIdWithIncludesAsync(
                reservation.Id,
                r => r.User,
                r => r.Table.Restaurant
            );

            _logger.LogInformation("Reservation created successfully with ID {ReservationId}", created.Id);

            return _mapper.Map<ReservationResponseDto>(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Attempting to delete reservation with ID {ReservationId}", id);

            if (!await _reservationRepository.ExistsAsync(id))
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found for deletion", id);
                return false;
            }

            await _reservationRepository.DeleteAsync(id);
            await _reservationRepository.SaveChangesAsync();

            _logger.LogInformation("Reservation with ID {ReservationId} deleted successfully", id);

            return true;
        }

        public async Task<List<ReservationResponseDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all reservations");

            var reservations = await _reservationRepository.GetAllWithIncludesAsync(
                r => r.User,
                r => r.Table.Restaurant
            );

            return _mapper.Map<List<ReservationResponseDto>>(reservations);
        }

        public async Task<ReservationResponseDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching reservation with ID {ReservationId}", id);

            var reservation = await _reservationRepository.GetByIdWithIncludesAsync(
                id,
                r => r.User,
                r => r.Table.Restaurant
            );

            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found", id);
                return null;
            }

            return _mapper.Map<ReservationResponseDto>(reservation);
        }

        public async Task<List<ReservationResponseDto>> GetByUserIdAsync(int userId)
        {
            _logger.LogInformation("Fetching reservations for user {UserId}", userId);

            if (!await _userRepository.ExistsAsync(userId))
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                throw new ArgumentException("User not found");
            }

            var reservations = await _reservationRepository.Query()
                .Where(r => r.UserId == userId)
                .Include(r => r.User)
                .Include(r => r.Table.Restaurant)
                .OrderByDescending(r => r.ReservationTime)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} reservations for user {UserId}", reservations.Count, userId);

            return _mapper.Map<List<ReservationResponseDto>>(reservations);
        }

        public async Task<List<ReservationResponseDto>> GetByRestaurantIdAsync(int restaurantId)
        {
            _logger.LogInformation("Fetching reservations for restaurant {RestaurantId}", restaurantId);

            if (!await _restaurantRepository.ExistsAsync(restaurantId))
            {
                _logger.LogWarning("Restaurant with ID {RestaurantId} not found", restaurantId);
                throw new ArgumentException("Restaurant not found");
            }

            var reservations = await _reservationRepository.Query()
                .Where(r => r.Table.RestaurantId == restaurantId)
                .Include(r => r.User)
                .Include(r => r.Table.Restaurant)
                .OrderByDescending(r => r.ReservationTime)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} reservations for restaurant {RestaurantId}", reservations.Count, restaurantId);

            return _mapper.Map<List<ReservationResponseDto>>(reservations);
        }

        public async Task<List<ReservationResponseDto>> GetByTableIdAsync(int tableId)
        {
            _logger.LogInformation("Fetching reservations for table {TableId}", tableId);

            if (!await _tableRepository.ExistsAsync(tableId))
            {
                _logger.LogWarning("Table with ID {TableId} not found", tableId);
                throw new ArgumentException("Table not found");
            }

            var reservations = await _reservationRepository.Query()
                .Where(r => r.TableId == tableId)
                .Include(r => r.User)
                .Include(r => r.Table.Restaurant)
                .OrderByDescending(r => r.ReservationTime)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} reservations for table {TableId}", reservations.Count, tableId);

            return _mapper.Map<List<ReservationResponseDto>>(reservations);
        }

        public async Task<List<ReservationResponseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("Fetching reservations between {StartDate} and {EndDate}", startDate, endDate);

            var reservations = await _reservationRepository.Query()
                .Where(r => r.ReservationTime >= startDate && r.ReservationTime <= endDate)
                .Include(r => r.User)
                .Include(r => r.Table.Restaurant)
                .OrderBy(r => r.ReservationTime)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} reservations in date range", reservations.Count);

            return _mapper.Map<List<ReservationResponseDto>>(reservations);
        }

        public async Task<List<ReservationResponseDto>> GetByStatusAsync(ReservationStatus status)
        {
            _logger.LogInformation("Fetching reservations with status {Status}", status);

            var reservations = await _reservationRepository.Query()
                .Where(r => r.Status == status)
                .Include(r => r.User)
                .Include(r => r.Table.Restaurant)
                .OrderByDescending(r => r.ReservationTime)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} reservations with status {Status}", reservations.Count, status);

            return _mapper.Map<List<ReservationResponseDto>>(reservations);
        }

        public async Task<ReservationResponseDto?> UpdateAsync(int id, UpdateReservationDto dto)
        {
            _logger.LogInformation("Updating reservation with ID {ReservationId}", id);

            var reservation = await _reservationRepository.GetByIdWithIncludesAsync(
                id,
                r => r.Table.Restaurant
            );

            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found for update", id);
                return null;
            }

            var restaurant = reservation.Table.Restaurant;

            var dayOfWeek = dto.ReservationTime.DayOfWeek;
            var dayFlag = (DayOfWeekFlags)(1 << (int)dayOfWeek);
            if (!restaurant.OperatingDays.HasFlag(dayFlag))
            {
                _logger.LogWarning("Update failed - restaurant {RestaurantId} is closed on {DayOfWeek}", restaurant.Id, dayOfWeek);
                throw new InvalidOperationException($"Restaurant is closed on {dayOfWeek}s");
            }

            var reservationTimeOnly = dto.ReservationTime.TimeOfDay;
            if (reservationTimeOnly < restaurant.OpeningTime || reservationTimeOnly > restaurant.ClosingTime)
            {
                _logger.LogWarning("Update failed - time {ReservationTime} is outside operating hours", reservationTimeOnly);
                throw new InvalidOperationException($"Restaurant operates from {restaurant.OpeningTime} to {restaurant.ClosingTime}");
            }

            var reservationEndTime = dto.ReservationTime.AddMinutes(restaurant.TableTurnoverMinutes);
            if (reservationEndTime.TimeOfDay > restaurant.ClosingTime)
            {
                _logger.LogWarning("Update failed - reservation end time {EndTime} exceeds closing time", reservationEndTime.TimeOfDay);
                throw new InvalidOperationException("This reservation would extend past closing time");
            }

            var hasOverlap = await _reservationRepository.AnyAsync(r =>
                r.TableId == reservation.TableId &&
                r.Id != id &&
                r.Status != ReservationStatus.Cancelled &&
                r.ReservationTime < reservationEndTime &&
                r.ReservationTime.AddMinutes(restaurant.TableTurnoverMinutes) > dto.ReservationTime
            );

            if (hasOverlap)
            {
                _logger.LogWarning("Update failed - time slot not available for reservation {ReservationId}", id);
                throw new InvalidOperationException("Time slot not available");
            }

            _mapper.Map(dto, reservation);

            await _reservationRepository.UpdateAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            var updated = await _reservationRepository.GetByIdWithIncludesAsync(
                id,
                r => r.User,
                r => r.Table.Restaurant
            );

            _logger.LogInformation("Reservation with ID {ReservationId} updated successfully", id);

            return _mapper.Map<ReservationResponseDto>(updated);
        }

        public async Task<bool> UpdateStatusAsync(int id, UpdateReservationStatusDto dto)
        {
            _logger.LogInformation("Updating status for reservation {ReservationId} to {Status}", id, dto.Status);

            var reservation = await _reservationRepository.GetByIdAsync(id);

            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found for status update", id);
                return false;
            }

            reservation.Status = dto.Status;

            await _reservationRepository.UpdateAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            _logger.LogInformation("Reservation {ReservationId} status updated to {Status}", id, dto.Status);

            return true;
        }

        public async Task<bool> CancelAsync(int id)
        {
            _logger.LogInformation("Attempting to cancel reservation with ID {ReservationId}", id);

            var reservation = await _reservationRepository.GetByIdAsync(id);

            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found for cancellation", id);
                return false;
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                _logger.LogWarning("Cancellation failed - reservation {ReservationId} is already cancelled", id);
                throw new InvalidOperationException("Reservation is already cancelled");
            }

            if (reservation.Status == ReservationStatus.Completed)
            {
                _logger.LogWarning("Cancellation failed - reservation {ReservationId} is already completed", id);
                throw new InvalidOperationException("Cannot cancel a completed reservation");
            }

            reservation.Status = ReservationStatus.Cancelled;

            await _reservationRepository.UpdateAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            _logger.LogInformation("Reservation {ReservationId} cancelled successfully", id);

            return true;
        }

        public async Task<PagedResult<ReservationResponseDto>> GetPagedAsync(int page, int pageSize)
        {
            _logger.LogInformation("Fetching paged reservations - page {Page}, size {PageSize}", page, pageSize);

            var query = _reservationRepository.Query()
                .Include(r => r.User)
                .Include(r => r.Table.Restaurant)
                .OrderByDescending(r => r.ReservationTime);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var reservations = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<ReservationResponseDto>>(reservations);

            _logger.LogInformation("Retrieved page {Page} of {TotalPages} with {Count} reservations", page, totalPages, reservations.Count);

            return new PagedResult<ReservationResponseDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }
    }
}
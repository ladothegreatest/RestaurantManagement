using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        public ReservationService(
            IMapper mapper,
            IRepository<Reservation> reservationRepository,
            IRepository<User> userRepository,
            IRepository<RestaurantTable> tableRepository,
            IRepository<Restaurant> restaurantRepository)
        {
            _mapper = mapper;
            _reservationRepository = reservationRepository;
            _userRepository = userRepository;
            _tableRepository = tableRepository;
            _restaurantRepository = restaurantRepository;
        }

        public async Task<ReservationResponseDto> CreateAsync(CreateReservationDto dto)
        {
            // momxmareblis validacia
            if (!await _userRepository.ExistsAsync(dto.UserId))
                throw new ArgumentException("User not found");

            // magidis validacia da restornis informaciis wamogeba
            var table = await _tableRepository.GetByIdWithIncludesAsync(dto.TableId, t => t.Restaurant);
            if (table == null)
                throw new ArgumentException("Table not found");

            var restaurant = table.Restaurant;
            if (restaurant == null)
                throw new ArgumentException("Restaurant not found");

            // aq mowmdeba restorani tu giaa
            if (!restaurant.IsOpen)
                throw new InvalidOperationException("Restaurant is currently closed");

            if (dto.NumberOfGuests > table.Capacity)
                throw new InvalidOperationException($"Table capacity is {table.Capacity}, but {dto.NumberOfGuests} guests requested");

            // samushao dgis validacia
            var dayOfWeek = dto.ReservationTime.DayOfWeek;
            var dayFlag = (DayOfWeekFlags)(1 << (int)dayOfWeek);
            if (!restaurant.OperatingDays.HasFlag(dayFlag))
                throw new InvalidOperationException($"Restaurant is closed on {dayOfWeek}s");

            // samushao saatebis validacia
            var reservationTimeOnly = dto.ReservationTime.TimeOfDay;
            if (reservationTimeOnly < restaurant.OpeningTime)
                throw new InvalidOperationException($"Restaurant opens at {restaurant.OpeningTime}. Please choose a later time.");

            if (reservationTimeOnly > restaurant.ClosingTime)
                throw new InvalidOperationException($"Restaurant closes at {restaurant.ClosingTime}. Please choose an earlier time.");

            // validacia rom javshnis saati ar cdeba mushaobis saatebs
            var reservationEndTime = dto.ReservationTime.AddMinutes(restaurant.TableTurnoverMinutes);
            if (reservationEndTime.TimeOfDay > restaurant.ClosingTime)
                throw new InvalidOperationException($"This reservation would extend past closing time ({restaurant.ClosingTime:hh\\:mm}). Please book earlier.");

            // validacia rom sxva javshani ar aris am dros
            var hasOverlap = await _reservationRepository.AnyAsync(r =>
                r.TableId == dto.TableId &&
                r.Status != ReservationStatus.Cancelled &&
                r.ReservationTime < reservationEndTime &&
                r.ReservationTime.AddMinutes(restaurant.TableTurnoverMinutes) > dto.ReservationTime
            );

            if (hasOverlap)
                throw new InvalidOperationException("This table is already reserved for this time slot. Please choose a different time.");

            var reservation = _mapper.Map<Reservation>(dto);

            await _reservationRepository.AddAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            var created = await _reservationRepository.GetByIdWithIncludesAsync(
                reservation.Id,
                r => r.User,
                r => r.Table.Restaurant
            );

            return _mapper.Map<ReservationResponseDto>(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _reservationRepository.ExistsAsync(id))
                return false;

            await _reservationRepository.DeleteAsync(id);
            await _reservationRepository.SaveChangesAsync();

            return true;
        }

        public async Task<List<ReservationResponseDto>> GetAllAsync()
        {
            var reservations = await _reservationRepository.GetAllWithIncludesAsync(
                r => r.User,
                r => r.Table.Restaurant
            );

            return _mapper.Map<List<ReservationResponseDto>>(reservations);
        }

        public async Task<ReservationResponseDto?> GetByIdAsync(int id)
        {
            var reservation = await _reservationRepository.GetByIdWithIncludesAsync(
                id,
                r => r.User,
                r => r.Table.Restaurant
            );

            if (reservation == null)
                return null;

            return _mapper.Map<ReservationResponseDto>(reservation);
        }

        public async Task<List<ReservationResponseDto>> GetByUserIdAsync(int userId)
        {
            if (!await _userRepository.ExistsAsync(userId))
                throw new ArgumentException("User not found");

            var reservations = await _reservationRepository.Query()
                .Where(r => r.UserId == userId)
                .Include(r => r.User)
                .Include(r => r.Table.Restaurant)
                .OrderByDescending(r => r.ReservationTime)
                .ToListAsync();

            return _mapper.Map<List<ReservationResponseDto>>(reservations);
        }

        public async Task<List<ReservationResponseDto>> GetByRestaurantIdAsync(int restaurantId)
        {
            if (!await _restaurantRepository.ExistsAsync(restaurantId))
                throw new ArgumentException("Restaurant not found");

            var reservations = await _reservationRepository.Query()
                .Where(r => r.Table.RestaurantId == restaurantId)
                .Include(r => r.User)
                .Include(r => r.Table.Restaurant)
                .OrderByDescending(r => r.ReservationTime)
                .ToListAsync();

            return _mapper.Map<List<ReservationResponseDto>>(reservations);
        }

        public async Task<List<ReservationResponseDto>> GetByTableIdAsync(int tableId)
        {
            if (!await _tableRepository.ExistsAsync(tableId))
                throw new ArgumentException("Table not found");

            var reservations = await _reservationRepository.Query()
                .Where(r => r.TableId == tableId)
                .Include(r => r.User)
                .Include(r => r.Table.Restaurant)
                .OrderByDescending(r => r.ReservationTime)
                .ToListAsync();

            return _mapper.Map<List<ReservationResponseDto>>(reservations);
        }

        public async Task<List<ReservationResponseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var reservations = await _reservationRepository.Query()
                .Where(r => r.ReservationTime >= startDate && r.ReservationTime <= endDate)
                .Include(r => r.User)
                .Include(r => r.Table.Restaurant)
                .OrderBy(r => r.ReservationTime)
                .ToListAsync();

            return _mapper.Map<List<ReservationResponseDto>>(reservations);
        }

        public async Task<List<ReservationResponseDto>> GetByStatusAsync(ReservationStatus status)
        {
            var reservations = await _reservationRepository.Query()
                .Where(r => r.Status == status)
                .Include(r => r.User)
                .Include(r => r.Table.Restaurant)
                .OrderByDescending(r => r.ReservationTime)
                .ToListAsync();

            return _mapper.Map<List<ReservationResponseDto>>(reservations);
        }

        public async Task<ReservationResponseDto?> UpdateAsync(int id, UpdateReservationDto dto)
        {
            var reservation = await _reservationRepository.GetByIdWithIncludesAsync(
                id,
                r => r.Table.Restaurant
            );

            if (reservation == null)
                return null;

            var restaurant = reservation.Table.Restaurant;

            // axali drois validacia
            var dayOfWeek = dto.ReservationTime.DayOfWeek;
            var dayFlag = (DayOfWeekFlags)(1 << (int)dayOfWeek);
            if (!restaurant.OperatingDays.HasFlag(dayFlag))
                throw new InvalidOperationException($"Restaurant is closed on {dayOfWeek}s");

            var reservationTimeOnly = dto.ReservationTime.TimeOfDay;
            if (reservationTimeOnly < restaurant.OpeningTime || reservationTimeOnly > restaurant.ClosingTime)
                throw new InvalidOperationException($"Restaurant operates from {restaurant.OpeningTime} to {restaurant.ClosingTime}");

            var reservationEndTime = dto.ReservationTime.AddMinutes(restaurant.TableTurnoverMinutes);
            if (reservationEndTime.TimeOfDay > restaurant.ClosingTime)
                throw new InvalidOperationException("This reservation would extend past closing time");

            // emtxveva tu ara sxva javshans
            var hasOverlap = await _reservationRepository.AnyAsync(r =>
                r.TableId == reservation.TableId &&
                r.Id != id &&
                r.Status != ReservationStatus.Cancelled &&
                r.ReservationTime < reservationEndTime &&
                r.ReservationTime.AddMinutes(restaurant.TableTurnoverMinutes) > dto.ReservationTime
            );

            if (hasOverlap)
                throw new InvalidOperationException("Time slot not available");

            _mapper.Map(dto, reservation);

            await _reservationRepository.UpdateAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            var updated = await _reservationRepository.GetByIdWithIncludesAsync(
                id,
                r => r.User,
                r => r.Table.Restaurant
            );

            return _mapper.Map<ReservationResponseDto>(updated);
        }

        public async Task<bool> UpdateStatusAsync(int id, UpdateReservationStatusDto dto)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);

            if (reservation == null)
                return false;

            reservation.Status = dto.Status;

            await _reservationRepository.UpdateAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelAsync(int id)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);

            if (reservation == null)
                return false;

            if (reservation.Status == ReservationStatus.Cancelled)
                throw new InvalidOperationException("Reservation is already cancelled");

            if (reservation.Status == ReservationStatus.Completed)
                throw new InvalidOperationException("Cannot cancel a completed reservation");

            reservation.Status = ReservationStatus.Cancelled;

            await _reservationRepository.UpdateAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<ReservationResponseDto>> GetPagedAsync(int page, int pageSize)
        {
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

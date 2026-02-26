using RestaurantManagement.Dtos.Reservation;
using RestaurantManagement.Entities;
using RestaurantManagement.Repositories;

namespace RestaurantManagement.Services;

public class ReservationService
{
    private readonly IRepository<Reservation> _repository;

    public ReservationService(IRepository<Reservation> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ReservationResponseDto>> GetAllAsync()
    {
        var reservations = await _repository.GetAllWithIncludesAsync(
            r => r.User!,
            r => r.Table!,
            r => r.Table!.Restaurant!
        );

        return reservations.Select(r => new ReservationResponseDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = r.User?.Name,
            UserEmail = r.User?.Email,

            TableId = r.TableId,
            TableNumber = r.Table?.TableNumber ?? 0,
            TableCapacity = r.Table?.Capacity ?? 0,

            RestaurantId = r.Table?.RestaurantId ?? 0,
            RestaurantName = r.Table?.Restaurant?.Name,

            ReservationTime = r.ReservationTime,
            NumberOfGuests = r.NumberOfGuests,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt
        });
    }

    public async Task CreateAsync(CreateReservationDto dto)
    {
        var reservation = new Reservation
        {
            UserId = dto.UserId,
            TableId = dto.TableId,
            ReservationTime = dto.ReservationTime,
            NumberOfGuests = dto.NumberOfGuests,
            Status = Common.Enums.ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(reservation);
        await _repository.SaveChangesAsync();
    }
}
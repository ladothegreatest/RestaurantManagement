using RestaurantManagement.Dtos.Restaurant;
using RestaurantManagement.Entities;
using RestaurantManagement.Repositories;

namespace RestaurantManagement.Services
{
    public class RestaurantService
    {
        private readonly IRepository<Restaurant> _repository;

        public RestaurantService(IRepository<Restaurant> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<RestaurantResponseDto>> GetAllAsync()
        {
            var restaurants = await _repository.GetAllWithIncludesAsync(r => r.Tables);

            return restaurants.Select(r => new RestaurantResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Address = r.Address,
                PhoneNumber = r.PhoneNumber,
                IsOpen = r.IsOpen,
                OpeningTime = r.OpeningTime,
                ClosingTime = r.ClosingTime,
                TableTurnoverMinutes = r.TableTurnoverMinutes,
                TotalTables = r.Tables?.Count ?? 0,
                AvailableTables = r.Tables?.Count(t => t.IsAvailable) ?? 0
            });
        }

        public async Task CreateAsync(CreateRestaurantDto dto)
        {
            var restaurant = new Restaurant
            {
                Name = dto.Name,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                IsOpen = dto.IsOpen,
                OpeningTime = dto.OpeningTime,
                ClosingTime = dto.ClosingTime,
                TableTurnoverMinutes = dto.TableTurnoverMinutes
            };

            await _repository.AddAsync(restaurant);
            await _repository.SaveChangesAsync();
        }
    }
}
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantManagement.Dtos.Restaurant;
using RestaurantManagement.Entities;
using RestaurantManagement.Repositories;
using RestaurantManagement.Services.Interfaces;

namespace RestaurantManagement.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Restaurant> _restaurantRepository;
        private readonly ILogger<RestaurantService> _logger;

        public RestaurantService(
            IMapper mapper,
            IRepository<Restaurant> restaurantRepository,
            ILogger<RestaurantService> logger)
        {
            _mapper = mapper;
            _restaurantRepository = restaurantRepository;
            _logger = logger;
        }

        public async Task<RestaurantResponseDto> CreateAsync(CreateRestaurantDto dto)
        {
            _logger.LogInformation("Creating restaurant with name {Name}", dto.Name);

            var restaurant = _mapper.Map<Restaurant>(dto);
            await _restaurantRepository.AddAsync(restaurant);
            await _restaurantRepository.SaveChangesAsync();

            var created = await _restaurantRepository.GetByIdWithIncludesAsync(restaurant.Id, r => r.Tables);

            _logger.LogInformation("Restaurant created successfully with ID {RestaurantId}", created.Id);

            return _mapper.Map<RestaurantResponseDto>(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Attempting to delete restaurant with ID {RestaurantId}", id);

            if (!await _restaurantRepository.ExistsAsync(id))
            {
                _logger.LogWarning("Restaurant with ID {RestaurantId} not found for deletion", id);
                return false;
            }

            await _restaurantRepository.DeleteAsync(id);
            await _restaurantRepository.SaveChangesAsync();

            _logger.LogInformation("Restaurant with ID {RestaurantId} deleted successfully", id);

            return true;
        }

        public async Task<List<RestaurantResponseDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all restaurants");

            var restaurants = await _restaurantRepository.GetAllWithIncludesAsync(r => r.Tables);

            return _mapper.Map<List<RestaurantResponseDto>>(restaurants);
        }

        public async Task<RestaurantResponseDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching restaurant with ID {RestaurantId}", id);

            var restaurant = await _restaurantRepository.GetByIdWithIncludesAsync(id, r => r.Tables);

            if (restaurant == null)
            {
                _logger.LogWarning("Restaurant with ID {RestaurantId} not found", id);
                return null;
            }

            return _mapper.Map<RestaurantResponseDto>(restaurant);
        }

        public async Task<List<RestaurantResponseDto>> GetOpenRestaurantsAsync()
        {
            _logger.LogInformation("Fetching all open restaurants");

            var restaurants = await _restaurantRepository.Query()
                .Where(r => r.IsOpen)
                .Include(r => r.Tables)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} open restaurants", restaurants.Count);

            return _mapper.Map<List<RestaurantResponseDto>>(restaurants);
        }

        public async Task<List<RestaurantResponseDto>> GetByNameAsync(string name)
        {
            _logger.LogInformation("Searching restaurants by name {Name}", name);

            var restaurants = await _restaurantRepository.Query()
                .Where(r => r.Name.Contains(name))
                .Include(r => r.Tables)
                .ToListAsync();

            _logger.LogInformation("Found {Count} restaurants matching name {Name}", restaurants.Count, name);

            return _mapper.Map<List<RestaurantResponseDto>>(restaurants);
        }

        public async Task<RestaurantResponseDto?> UpdateAsync(int id, UpdateRestaurantDto dto)
        {
            _logger.LogInformation("Updating restaurant with ID {RestaurantId}", id);

            var restaurant = await _restaurantRepository.GetByIdAsync(id);

            if (restaurant == null)
            {
                _logger.LogWarning("Restaurant with ID {RestaurantId} not found for update", id);
                return null;
            }

            _mapper.Map(dto, restaurant);
            await _restaurantRepository.UpdateAsync(restaurant);
            await _restaurantRepository.SaveChangesAsync();

            var updated = await _restaurantRepository.GetByIdWithIncludesAsync(id, r => r.Tables);

            _logger.LogInformation("Restaurant with ID {RestaurantId} updated successfully", id);

            return _mapper.Map<RestaurantResponseDto>(updated);
        }

        public async Task<bool> ToggleOpenStatusAsync(int id)
        {
            _logger.LogInformation("Toggling open status for restaurant with ID {RestaurantId}", id);

            var restaurant = await _restaurantRepository.GetByIdAsync(id);

            if (restaurant == null)
            {
                _logger.LogWarning("Restaurant with ID {RestaurantId} not found for status toggle", id);
                return false;
            }

            restaurant.IsOpen = !restaurant.IsOpen;
            await _restaurantRepository.UpdateAsync(restaurant);
            await _restaurantRepository.SaveChangesAsync();

            _logger.LogInformation("Restaurant {RestaurantId} is now {Status}", id, restaurant.IsOpen ? "Open" : "Closed");

            return true;
        }
    }
}
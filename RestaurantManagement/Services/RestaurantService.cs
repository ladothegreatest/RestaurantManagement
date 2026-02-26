using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        public RestaurantService(IMapper mapper, IRepository<Restaurant> restaurantRepository)
        {
            _mapper = mapper;
            _restaurantRepository = restaurantRepository;
        }

        public async Task<RestaurantResponseDto> CreateAsync(CreateRestaurantDto dto)
        {
            var restaurant = _mapper.Map<Restaurant>(dto);

            await _restaurantRepository.AddAsync(restaurant);
            await _restaurantRepository.SaveChangesAsync();

            var created = await _restaurantRepository.GetByIdWithIncludesAsync(restaurant.Id, r => r.Tables);

            return _mapper.Map<RestaurantResponseDto>(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _restaurantRepository.ExistsAsync(id))
                return false;

            await _restaurantRepository.DeleteAsync(id);
            await _restaurantRepository.SaveChangesAsync();

            return true;
        }

        public async Task<List<RestaurantResponseDto>> GetAllAsync()
        {
            var restaurants = await _restaurantRepository.GetAllWithIncludesAsync(r => r.Tables);

            return _mapper.Map<List<RestaurantResponseDto>>(restaurants);
        }

        public async Task<RestaurantResponseDto?> GetByIdAsync(int id)
        {
            var restaurant = await _restaurantRepository.GetByIdWithIncludesAsync(id, r => r.Tables);

            if (restaurant == null)
                return null;

            return _mapper.Map<RestaurantResponseDto>(restaurant);
        }

        public async Task<List<RestaurantResponseDto>> GetOpenRestaurantsAsync()
        {
            var restaurants = await _restaurantRepository.Query()
                .Where(r => r.IsOpen)
                .Include(r => r.Tables)
                .ToListAsync();

            return _mapper.Map<List<RestaurantResponseDto>>(restaurants);
        }

        public async Task<List<RestaurantResponseDto>> GetByNameAsync(string name)
        {
            var restaurants = await _restaurantRepository.Query()
                .Where(r => r.Name.Contains(name))
                .Include(r => r.Tables)
                .ToListAsync();

            return _mapper.Map<List<RestaurantResponseDto>>(restaurants);
        }

        public async Task<RestaurantResponseDto?> UpdateAsync(int id, UpdateRestaurantDto dto)
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(id);

            if (restaurant == null)
                return null;

            _mapper.Map(dto, restaurant);

            await _restaurantRepository.UpdateAsync(restaurant);
            await _restaurantRepository.SaveChangesAsync();

            var updated = await _restaurantRepository.GetByIdWithIncludesAsync(id, r => r.Tables);

            return _mapper.Map<RestaurantResponseDto>(updated);
        }

        public async Task<bool> ToggleOpenStatusAsync(int id)
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(id);

            if (restaurant == null)
                return false;

            restaurant.IsOpen = !restaurant.IsOpen;

            await _restaurantRepository.UpdateAsync(restaurant);
            await _restaurantRepository.SaveChangesAsync();

            return true;
        }
    }
}   

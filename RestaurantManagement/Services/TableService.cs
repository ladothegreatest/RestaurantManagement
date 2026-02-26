using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Dtos.Table;
using RestaurantManagement.Entities;
using RestaurantManagement.Repositories;
using RestaurantManagement.Services.Interfaces;

namespace RestaurantManagement.Services
{
    public class TableService : ITableService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<RestaurantTable> _tableRepository;
        private readonly IRepository<Restaurant> _restaurantRepository;

        public TableService(IMapper mapper, IRepository<RestaurantTable> tableRepository, IRepository<Restaurant> restaurantRepository)
        {
            _mapper = mapper;
            _tableRepository = tableRepository;
            _restaurantRepository = restaurantRepository;
        }

        public async Task<TableResponseDto> CreateAsync(CreateTableDto dto)
        {
            if (!await _restaurantRepository.ExistsAsync(dto.RestaurantId))
                throw new ArgumentException("Restaurant not found");

            var tableExists = await _tableRepository.AnyAsync(t =>
                t.RestaurantId == dto.RestaurantId &&
                t.TableNumber == dto.TableNumber);

            if (tableExists)
                throw new InvalidOperationException($"Table number {dto.TableNumber} already exists in this restaurant");

            var table = _mapper.Map<RestaurantTable>(dto);

            await _tableRepository.AddAsync(table);
            await _tableRepository.SaveChangesAsync();

            var created = await _tableRepository.GetByIdWithIncludesAsync(table.Id, t => t.Restaurant);

            return _mapper.Map<TableResponseDto>(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _tableRepository.ExistsAsync(id))
                return false;

            await _tableRepository.DeleteAsync(id);
            await _tableRepository.SaveChangesAsync();

            return true;
        }

        public async Task<List<TableResponseDto>> GetAllAsync()
        {
            var tables = await _tableRepository.GetAllWithIncludesAsync(t => t.Restaurant);

            return _mapper.Map<List<TableResponseDto>>(tables);
        }

        public async Task<TableResponseDto?> GetByIdAsync(int id)
        {
            var table = await _tableRepository.GetByIdWithIncludesAsync(id, t => t.Restaurant);

            if (table == null)
                return null;

            return _mapper.Map<TableResponseDto>(table);
        }

        public async Task<List<TableResponseDto>> GetByRestaurantIdAsync(int restaurantId)
        {
            if (!await _restaurantRepository.ExistsAsync(restaurantId))
                throw new ArgumentException("Restaurant not found");

            var tables = await _tableRepository.Query()
                .Where(t => t.RestaurantId == restaurantId)
                .Include(t => t.Restaurant)
                .ToListAsync();

            return _mapper.Map<List<TableResponseDto>>(tables);
        }

        public async Task<List<TableResponseDto>> GetAvailableTablesByRestaurantIdAsync(int restaurantId)
        {
            if (!await _restaurantRepository.ExistsAsync(restaurantId))
                throw new ArgumentException("Restaurant not found");

            var tables = await _tableRepository.Query()
                .Where(t => t.RestaurantId == restaurantId && t.IsAvailable)
                .Include(t => t.Restaurant)
                .ToListAsync();

            return _mapper.Map<List<TableResponseDto>>(tables);
        }

        public async Task<List<TableResponseDto>> GetByCapacityAsync(int restaurantId, int minCapacity)
        {
            if (!await _restaurantRepository.ExistsAsync(restaurantId))
                throw new ArgumentException("Restaurant not found");

            var tables = await _tableRepository.Query()
                .Where(t => t.RestaurantId == restaurantId && t.Capacity >= minCapacity)
                .Include(t => t.Restaurant)
                .ToListAsync();

            return _mapper.Map<List<TableResponseDto>>(tables);
        }

        public async Task<TableResponseDto?> UpdateAsync(int id, UpdateTableDto dto)
        {
            var table = await _tableRepository.GetByIdAsync(id);

            if (table == null)
                return null;

            var tableExists = await _tableRepository.AnyAsync(t =>
                t.RestaurantId == table.RestaurantId &&
                t.TableNumber == dto.TableNumber &&
                t.Id != id);

            if (tableExists)
                throw new InvalidOperationException($"Table number {dto.TableNumber} already exists in this restaurant");

            _mapper.Map(dto, table);

            await _tableRepository.UpdateAsync(table);
            await _tableRepository.SaveChangesAsync();

            var updated = await _tableRepository.GetByIdWithIncludesAsync(id, t => t.Restaurant);

            return _mapper.Map<TableResponseDto>(updated);
        }

        public async Task<bool> ToggleAvailabilityAsync(int id)
        {
            var table = await _tableRepository.GetByIdAsync(id);

            if (table == null)
                return false;

            table.IsAvailable = !table.IsAvailable;

            await _tableRepository.UpdateAsync(table);
            await _tableRepository.SaveChangesAsync();

            return true;
        }
    }
}

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<TableService> _logger;

        public TableService(
            IMapper mapper,
            IRepository<RestaurantTable> tableRepository,
            IRepository<Restaurant> restaurantRepository,
            ILogger<TableService> logger)
        {
            _mapper = mapper;
            _tableRepository = tableRepository;
            _restaurantRepository = restaurantRepository;
            _logger = logger;
        }

        public async Task<TableResponseDto> CreateAsync(CreateTableDto dto)
        {
            _logger.LogInformation("Creating table number {TableNumber} for restaurant {RestaurantId}", dto.TableNumber, dto.RestaurantId);

            if (!await _restaurantRepository.ExistsAsync(dto.RestaurantId))
            {
                _logger.LogWarning("Restaurant with ID {RestaurantId} not found", dto.RestaurantId);
                throw new ArgumentException("Restaurant not found");
            }

            var tableExists = await _tableRepository.AnyAsync(t =>
                t.RestaurantId == dto.RestaurantId &&
                t.TableNumber == dto.TableNumber);

            if (tableExists)
            {
                _logger.LogWarning("Table number {TableNumber} already exists in restaurant {RestaurantId}", dto.TableNumber, dto.RestaurantId);
                throw new InvalidOperationException($"Table number {dto.TableNumber} already exists in this restaurant");
            }

            var table = _mapper.Map<RestaurantTable>(dto);

            await _tableRepository.AddAsync(table);
            await _tableRepository.SaveChangesAsync();

            var created = await _tableRepository.GetByIdWithIncludesAsync(table.Id, t => t.Restaurant);

            _logger.LogInformation("Table created successfully with ID {TableId}", created.Id);

            return _mapper.Map<TableResponseDto>(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Attempting to delete table with ID {TableId}", id);

            if (!await _tableRepository.ExistsAsync(id))
            {
                _logger.LogWarning("Table with ID {TableId} not found for deletion", id);
                return false;
            }

            await _tableRepository.DeleteAsync(id);
            await _tableRepository.SaveChangesAsync();

            _logger.LogInformation("Table with ID {TableId} deleted successfully", id);

            return true;
        }

        public async Task<List<TableResponseDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all tables");

            var tables = await _tableRepository.GetAllWithIncludesAsync(t => t.Restaurant);

            return _mapper.Map<List<TableResponseDto>>(tables);
        }

        public async Task<TableResponseDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching table with ID {TableId}", id);

            var table = await _tableRepository.GetByIdWithIncludesAsync(id, t => t.Restaurant);

            if (table == null)
            {
                _logger.LogWarning("Table with ID {TableId} not found", id);
                return null;
            }

            return _mapper.Map<TableResponseDto>(table);
        }

        public async Task<List<TableResponseDto>> GetByRestaurantIdAsync(int restaurantId)
        {
            _logger.LogInformation("Fetching tables for restaurant {RestaurantId}", restaurantId);

            if (!await _restaurantRepository.ExistsAsync(restaurantId))
            {
                _logger.LogWarning("Restaurant with ID {RestaurantId} not found", restaurantId);
                throw new ArgumentException("Restaurant not found");
            }

            var tables = await _tableRepository.Query()
                .Where(t => t.RestaurantId == restaurantId)
                .Include(t => t.Restaurant)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} tables for restaurant {RestaurantId}", tables.Count, restaurantId);

            return _mapper.Map<List<TableResponseDto>>(tables);
        }

        public async Task<List<TableResponseDto>> GetAvailableTablesByRestaurantIdAsync(int restaurantId)
        {
            _logger.LogInformation("Fetching available tables for restaurant {RestaurantId}", restaurantId);

            if (!await _restaurantRepository.ExistsAsync(restaurantId))
            {
                _logger.LogWarning("Restaurant with ID {RestaurantId} not found", restaurantId);
                throw new ArgumentException("Restaurant not found");
            }

            var tables = await _tableRepository.Query()
                .Where(t => t.RestaurantId == restaurantId && t.IsAvailable)
                .Include(t => t.Restaurant)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} available tables for restaurant {RestaurantId}", tables.Count, restaurantId);

            return _mapper.Map<List<TableResponseDto>>(tables);
        }

        public async Task<List<TableResponseDto>> GetByCapacityAsync(int restaurantId, int minCapacity)
        {
            _logger.LogInformation("Fetching tables with min capacity {MinCapacity} for restaurant {RestaurantId}", minCapacity, restaurantId);

            if (!await _restaurantRepository.ExistsAsync(restaurantId))
            {
                _logger.LogWarning("Restaurant with ID {RestaurantId} not found", restaurantId);
                throw new ArgumentException("Restaurant not found");
            }

            var tables = await _tableRepository.Query()
                .Where(t => t.RestaurantId == restaurantId && t.Capacity >= minCapacity)
                .Include(t => t.Restaurant)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} tables with capacity >= {MinCapacity} for restaurant {RestaurantId}", tables.Count, minCapacity, restaurantId);

            return _mapper.Map<List<TableResponseDto>>(tables);
        }

        public async Task<TableResponseDto?> UpdateAsync(int id, UpdateTableDto dto)
        {
            _logger.LogInformation("Updating table with ID {TableId}", id);

            var table = await _tableRepository.GetByIdAsync(id);

            if (table == null)
            {
                _logger.LogWarning("Table with ID {TableId} not found for update", id);
                return null;
            }

            var tableExists = await _tableRepository.AnyAsync(t =>
                t.RestaurantId == table.RestaurantId &&
                t.TableNumber == dto.TableNumber &&
                t.Id != id);

            if (tableExists)
            {
                _logger.LogWarning("Table number {TableNumber} already exists in restaurant {RestaurantId}", dto.TableNumber, table.RestaurantId);
                throw new InvalidOperationException($"Table number {dto.TableNumber} already exists in this restaurant");
            }

            _mapper.Map(dto, table);

            await _tableRepository.UpdateAsync(table);
            await _tableRepository.SaveChangesAsync();

            var updated = await _tableRepository.GetByIdWithIncludesAsync(id, t => t.Restaurant);

            _logger.LogInformation("Table with ID {TableId} updated successfully", id);

            return _mapper.Map<TableResponseDto>(updated);
        }

        public async Task<bool> ToggleAvailabilityAsync(int id)
        {
            _logger.LogInformation("Toggling availability for table with ID {TableId}", id);

            var table = await _tableRepository.GetByIdAsync(id);

            if (table == null)
            {
                _logger.LogWarning("Table with ID {TableId} not found for availability toggle", id);
                return false;
            }

            table.IsAvailable = !table.IsAvailable;

            await _tableRepository.UpdateAsync(table);
            await _tableRepository.SaveChangesAsync();

            _logger.LogInformation("Table {TableId} availability is now {IsAvailable}", id, table.IsAvailable);

            return true;
        }
    }
}
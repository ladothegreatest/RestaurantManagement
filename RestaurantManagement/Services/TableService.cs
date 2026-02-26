using RestaurantManagement.Dtos.Table;
using RestaurantManagement.Entities;
using RestaurantManagement.Repositories;

namespace RestaurantManagement.Services
{
    public class TableService
    {
        private readonly IRepository<RestaurantTable> _repository;

        public TableService(IRepository<RestaurantTable> repository)
        {
            _repository = repository;
        }

        public async Task CreateAsync(CreateTableDto dto)
        {
            var table = new RestaurantTable
            {
                TableNumber = dto.TableNumber,
                Capacity = dto.Capacity,
                RestaurantId = dto.RestaurantId,
                IsAvailable = true
            };

            await _repository.AddAsync(table);
            await _repository.SaveChangesAsync();
        }
    }
}
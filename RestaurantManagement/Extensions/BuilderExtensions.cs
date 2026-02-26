using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Repositories;
using RestaurantManagement.Services;

namespace RestaurantManagement.Extensions
{
    public static class BuilderExtensions
    {
        public static void ConfigureDatabase(this WebApplicationBuilder builder)
        {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        }
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<RestaurantService>();
            builder.Services.AddScoped<TableService>();
            builder.Services.AddScoped<ReservationService>();
        }
    }
}

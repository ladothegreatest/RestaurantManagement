using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;

namespace RestaurantManagement.Extensions
{
    public static class BuilderExtensions
    {
        public static void ConfigureDatabase(this WebApplicationBuilder builder)
        {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        }
    }
}

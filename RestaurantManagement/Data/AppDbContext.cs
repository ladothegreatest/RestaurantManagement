using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace RestaurantManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.User> Users { get; set; }

        public DbSet<Entities.Restaurant> Restaurants { get; set; }

        public DbSet<Entities.RestaurantTable> RestaurantTables { get; set; }

        public DbSet<Entities.Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}

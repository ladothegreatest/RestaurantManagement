using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantManagement.Common.Enums;
using RestaurantManagement.Entities;

namespace RestaurantManagement.Data.Configs
{
    public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
    {
        public void Configure(EntityTypeBuilder<Restaurant> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Address)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(r => r.OpeningTime)
                .IsRequired();

            builder.Property(r => r.ClosingTime)
                .IsRequired();

            builder.Property(r => r.OperatingDays)
                .IsRequired()
                .HasDefaultValue(DayOfWeekFlags.All);

            builder.Property(r => r.TableTurnoverMinutes)
                .IsRequired()
                .HasDefaultValue(120);

            builder.HasMany(r => r.Tables)
                .WithOne(t => t.Restaurant)
                .HasForeignKey(t => t.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantManagement.Entities;

namespace RestaurantManagement.Data.Configs
{
    public class RestaurantTableConfiguration : IEntityTypeConfiguration<RestaurantTable>
    {
        public void Configure(EntityTypeBuilder<RestaurantTable> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.TableNumber)
                .IsRequired();

            builder.Property(rt => rt.Capacity)
                .IsRequired();

            builder.Property(rt => rt.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasOne(rt => rt.Restaurant)
                .WithMany(r => r.Tables)
                .HasForeignKey(rt => rt.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(rt => rt.Reservations)
                .WithOne(r => r.Table)
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(rt => new { rt.RestaurantId, rt.TableNumber })
                .IsUnique();
        }
    }
}

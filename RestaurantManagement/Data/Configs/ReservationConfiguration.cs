using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantManagement.Common.Enums;
using RestaurantManagement.Entities;

namespace RestaurantManagement.Data.Configs
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.ReservationTime)
                .IsRequired();

            builder.Property(r => r.NumberOfGuests)
                .IsRequired();

            builder.Property(r => r.Status)
                .IsRequired()
                .HasDefaultValue(ReservationStatus.Pending);

            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETDATE()")
                .ValueGeneratedOnAdd();

            builder.HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.TableId, r.ReservationTime });
        }
    }
}

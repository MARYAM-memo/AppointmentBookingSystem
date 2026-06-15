using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppointmentBooking.Core.Models;

namespace AppointmentBooking.Infrastructure.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Email)
            .HasMaxLength(200);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        builder.Property(c => c.TotalAppointments)
            .HasDefaultValue(0);

        builder.Property(c => c.CreatedAt)
            .IsRequired().HasColumnType("date");

        builder.Property(c => c.UpdatedAt)
            .IsRequired(false);

        builder.Property(c => c.LastAppointmentDate)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(c => c.PhoneNumber)
            .IsUnique();

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.HasIndex(c => c.FullName);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

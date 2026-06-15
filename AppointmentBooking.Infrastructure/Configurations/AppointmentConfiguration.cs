using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppointmentBooking.Core.Models;

namespace AppointmentBooking.Infrastructure.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AppointmentDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(a => a.StartTime)
            .IsRequired()
            .HasColumnType("time");

        builder.Property(a => a.EndTime)
            .IsRequired()
            .HasColumnType("time");

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasDefaultValue(BookingStatus.Pending);

        builder.Property(a => a.TotalPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.FinalPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.Currency)
            .HasMaxLength(10);

        builder.Property(a => a.CreatedAt)
            .IsRequired().HasColumnType("date");

        builder.Property(a => a.UpdatedAt)
            .IsRequired(false);

        builder.Property(a => a.CreatedByUserId)
            .HasMaxLength(100);

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a => a.RowVersion)
        .IsRowVersion();

        // Soft Delete Query Filter
        builder.HasQueryFilter(a => !a.IsDeleted);

        // Indexes for performance
        builder.HasIndex(a => new { a.AppointmentDate, a.StartTime, a.EndTime });
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.AppointmentDate);
        builder.HasIndex(a => new { a.ServiceId, a.AppointmentDate });
        builder.HasIndex(a => a.Status).HasDatabaseName("IX_appointment_status");
        builder.HasIndex(a => a.ServiceId).IsUnique(false).HasDatabaseName("IX_appointment_service");
        builder.HasIndex(a => a.CustomerId).IsUnique(false).HasDatabaseName("IX_appointment_customer");
        builder.HasIndex(a => new { a.AppointmentDate, a.ServiceId }).HasDatabaseName("IX_appointment_date_service");
        builder.HasIndex(a => new { a.AppointmentDate, a.StartTime, a.EndTime, a.Status }).HasDatabaseName("IX_Appointments_Availability");
        builder.HasIndex(a => new { a.ServiceId, a.AppointmentDate, a.StartTime }).IsUnique().HasDatabaseName("IX_Appointments_NoDoubleBooking").HasFilter("[IsDeleted] = 0 AND [Status] != 2"); ;

        // Relationships
        builder.HasOne(a => a.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

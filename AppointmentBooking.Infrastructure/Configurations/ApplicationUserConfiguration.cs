using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppointmentBooking.Infrastructure.Identity;

namespace AppointmentBooking.Infrastructure.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> entity)
    {
        // Properties
        entity.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.DateOfBirth)
            .HasColumnType("date")
            .IsRequired(false);

        entity.Property(e => e.Gender)
            .HasConversion<int>()
            .IsRequired(false);

        entity.Property(e => e.PreferDarkMode)
            .HasDefaultValue(false);

        entity.Property(e => e.PreferredLanguage)
            .HasMaxLength(10)
            .HasDefaultValue("ar");

        entity.Property(e => e.EnableNotifications)
            .HasDefaultValue(true);

        entity.Property(e => e.CreatedAt)
            .IsRequired().HasColumnType("date");

        entity.Property(e => e.UpdatedAt)
            .IsRequired(false);

        entity.Property(e => e.LastLoginAt)
            .IsRequired(false);

        // Ignore computed properties
        entity.Ignore(e => e.DisplayName);
        entity.Ignore(e => e.Age);
        entity.Ignore(e => e.JoinDate);

        // Indexes
        entity.HasIndex(e => e.FullName)
            .HasDatabaseName("IX_ApplicationUser_FullName");

        entity.HasIndex(e => e.DateOfBirth)
            .HasDatabaseName("IX_ApplicationUser_DateOfBirth");
    }
}

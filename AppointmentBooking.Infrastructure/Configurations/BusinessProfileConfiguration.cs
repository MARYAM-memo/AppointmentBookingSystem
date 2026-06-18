using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppointmentBooking.Core.Models;
using System.Text.Json;
using AppointmentBooking.Infrastructure.Comparers;

namespace AppointmentBooking.Infrastructure.Configurations;

public class BusinessProfileConfiguration : IEntityTypeConfiguration<BusinessProfile>
{
    public void Configure(EntityTypeBuilder<BusinessProfile> builder)
    {
        builder.ToTable("BusinessProfiles");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BusinessName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.BusinessType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.Tagline)
            .HasMaxLength(500);

        builder.Property(b => b.LogoUrl)
            .HasMaxLength(500);

        builder.Property(b => b.FaviconUrl)
            .HasMaxLength(500);

        // Configuring Complex Types (Owned Entities)
        builder.OwnsOne(b => b.Colors, colors =>
        {
            colors.Property(c => c.Primary)
                .HasMaxLength(20);

            colors.Property(c => c.Secondary)
                .HasMaxLength(20);

            colors.Property(c => c.Accent)
                .HasMaxLength(20);

            colors.Property(c => c.DarkModePrimary)
                .HasMaxLength(20);
        });

        builder.OwnsOne(b => b.Localization, localization =>
        {
            localization.Property(l => l.Currency)
                .HasMaxLength(10);

            localization.Property(l => l.Language)
                .HasMaxLength(10);

            localization.Property(l => l.Direction)
                .HasMaxLength(10);
        });

        builder.OwnsOne(b => b.Contact, contact =>
        {
            contact.Property(c => c.Phone)
                .HasMaxLength(20);

            contact.Property(c => c.Email)
                .HasMaxLength(200);

            contact.Property(c => c.Address)
                .HasMaxLength(500);
        });

        // Store Dictionary as JSON
        builder.Property(bp => bp.CustomLabels)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new()
            )
            .Metadata.SetValueComparer(ValueComparers.GetDictionaryComparer());


        builder.Property(b => b.WorkingHoursStart).IsRequired();
        builder.Property(b => b.WorkingHoursEnd).IsRequired();

        builder.Property(b => b.SlotDurationMinutes);

        builder.Property(b => b.CreatedAt)
            .IsRequired().HasColumnType("date");

        builder.Property(b => b.UpdatedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(b => b.BusinessName);
        builder.HasIndex(b => b.BusinessType);
        builder.HasIndex(b => b.Id).IsUnique();
    }
}


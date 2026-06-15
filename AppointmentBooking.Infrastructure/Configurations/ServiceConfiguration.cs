using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppointmentBooking.Core.Models;
using System.Text.Json;
using AppointmentBooking.Infrastructure.Comparers;

namespace AppointmentBooking.Infrastructure.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(2000);

        builder.Property(s => s.Category)
            .HasMaxLength(100);

        builder.Property(s => s.SubCategory)
            .HasMaxLength(100);

        builder.Property(s => s.Duration)
            .IsRequired()
            .HasDefaultValue(TimeSpan.FromMinutes(30));

        builder.Property(s => s.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.PricePerAdditionalHour)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.BufferBefore);

        builder.Property(s => s.BufferAfter);

        builder.Property(s => s.MaxCapacity)
            .HasDefaultValue(1);

        // Store List as JSON
        builder.Property(s => s.RequiredDocuments)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v != null && v.Any() ? JsonSerializer.Serialize(v) : null,
                v => !string.IsNullOrEmpty(v) ? JsonSerializer.Deserialize<List<string>>(v) : null
            )
            .Metadata.SetValueComparer(ValueComparers.GetStringListComparer()); ;

        builder.Property(s => s.Icon)
            .HasMaxLength(100);

        builder.Property(s => s.Color)
            .HasMaxLength(20);

        builder.Property(s => s.ImageUrl)
            .HasMaxLength(500);

        builder.Property(s => s.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.IsPopular)
            .HasDefaultValue(false);

        builder.Property(s => s.CreatedAt)
            .IsRequired().HasColumnType("date");

        builder.Property(s => s.UpdatedAt)
            .IsRequired(false);

        // Soft Delete Query Filter
        builder.HasQueryFilter(s => !s.IsDeleted);

        // Indexes
        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => new { s.Category, s.SubCategory });
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.IsPopular);
        builder.HasIndex(s => s.DisplayOrder);
    }
}

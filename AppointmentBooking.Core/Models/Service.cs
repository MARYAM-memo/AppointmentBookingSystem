using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppointmentBooking.Core.Abstractions;

namespace AppointmentBooking.Core.Models;

public class Service : IEntity, ISoftDelete
{
      [Key]
      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public int Id { get; set; }

      //Basics
      public string Name { get; set; } = string.Empty;
      public string? Description { get; set; }
      public string? Category { get; set; }
      public string? SubCategory { get; set; }

      // Duration and price
      public TimeSpan Duration { get; set; }
      public decimal Price { get; set; }
      public decimal? PricePerAdditionalHour { get; set; }// for long services

      // Buffer times (cleaning/preparation between appointments)
      public TimeSpan? BufferBefore { get; set; }
      public TimeSpan? BufferAfter { get; set; }

      // Capacity (if more than one person is using it at the same time)
      public int MaxCapacity { get; set; } = 1;

      // Requireds
      public List<string>? RequiredDocuments { get; set; } // documents

      // Display
      public string? Icon { get; set; } // Bootstrap icon class
      public string? Color { get; set; } // Custom color for the service
      public string? ImageUrl { get; set; }
      public int DisplayOrder { get; set; } = 0;
      public bool IsActive { get; set; } = true;
      public bool IsPopular { get; set; }

      // Audit
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? UpdatedAt { get; set; }

      // Soft Delete
      public bool IsDeleted { get; set; }
      public DateTime? DeletedAt { get; set; }

      // Navigation
      public ICollection<Appointment>? Appointments { get; set; } = default;

      // ========== Domain Methods ==========
      /// <summary>
      /// Total duration (Service + Buffers)
      /// </summary>
      public TimeSpan GetTotalDuration()
      {
            return Duration
                .Add(BufferBefore ?? TimeSpan.Zero)
                .Add(BufferAfter ?? TimeSpan.Zero);
      }
}

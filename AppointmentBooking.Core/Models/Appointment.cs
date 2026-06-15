using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppointmentBooking.Core.Abstractions;

namespace AppointmentBooking.Core.Models;

public enum BookingStatus
{
      Pending = 0,
      Confirmed = 1,
      InProgress = 2,
      Completed = 3,
      Cancelled = 4,
      NoShow = 5,
      Rescheduled = 6
}
public class Appointment : IEntity, ISoftDelete
{
      [Key]
      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public int Id { get; set; }

      // Appointments details
      public DateTime AppointmentDate { get; set; }
      public TimeSpan StartTime { get; set; }
      public TimeSpan EndTime { get; set; }
      public BookingStatus Status { get; set; } = BookingStatus.Pending;

      // Regarding price (each business decides)
      public decimal? TotalPrice { get; set; }
      public decimal? DiscountAmount { get; set; }
      public decimal? FinalPrice { get; set; }
      public string? Currency { get; set; }

      // Audit
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? UpdatedAt { get; set; }
      public string? CreatedByUserId { get; set; }

      // Soft Delete
      public bool IsDeleted { get; set; }
      public DateTime? DeletedAt { get; set; }

      // extra
      public string? Notes { get; set; }

      //Relationships
      public int ServiceId { get; set; }
      public Service Service { get; set; } = null!;

      public int CustomerId { get; set; }
      public Customer Customer { get; set; } = null!;

      [Timestamp]
      public byte[]? RowVersion { get; set; }

      // ========== Domain Methods ==========
      /// <summary>
      ///EndTime is calculated based on service duration and buffer.
      /// </summary>
      public void CalculateEndTime(TimeSpan serviceDuration, TimeSpan? bufferBefore = null,  TimeSpan? bufferAfter = null)
      {
            if (StartTime == default)
                  throw new InvalidOperationException("StartTime must be set before calculating EndTime.");

            // EndTime = StartTime + BufferBefore + Duration + BufferAfter
            EndTime = StartTime
                .Add(bufferBefore ?? TimeSpan.Zero)
                .Add(serviceDuration)
                .Add(bufferAfter ?? TimeSpan.Zero);
      }

      /// <summary>
      /// Overload takes up the entire service.
      /// </summary>
      public void CalculateEndTime(Service service)
      {
            ArgumentNullException.ThrowIfNull(service);

            CalculateEndTime(service.Duration, service.BufferBefore, service.BufferAfter);
      }
}

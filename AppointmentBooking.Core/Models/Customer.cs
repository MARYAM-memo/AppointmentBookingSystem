using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppointmentBooking.Core.Abstractions;

namespace AppointmentBooking.Core.Models;

public class Customer : IEntity, ISoftDelete
{
      [Key]
      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public int Id { get; set; }
      public string FullName { get; set; } = string.Empty;
      public string PhoneNumber { get; set; } = string.Empty;
      public string? Email { get; set; }

      // extra
      public string? Notes { get; set; }
      public int TotalAppointments { get; set; }

      // Audit
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? UpdatedAt { get; set; }
      public DateTime? LastAppointmentDate { get; set; }

      //Soft Delete
      public bool IsDeleted { get; set; }
      public DateTime? DeletedAt { get; set; }

      // Navigation
      public ICollection<Appointment>? Appointments { get; set; } = default;

      // ========== Domain Methods ==========
      /// <summary>
      /// The number of appointments increases
      /// </summary>
      public void IncrementAppointments()
      {
            TotalAppointments++;
            LastAppointmentDate = DateTime.UtcNow;
      }

      /// <summary>
      /// Reduces the number of appointments (if a appointment is cancelled)
      /// </summary>
      public void DecrementAppointments()
      {
            if (TotalAppointments > 0)
                  TotalAppointments--;
      }
}

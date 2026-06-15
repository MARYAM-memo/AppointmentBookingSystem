using AppointmentBooking.Core.Models;

namespace AppointmentBooking.Application.ViewModels.Appointment;

public class AppointmentRequestViewModel
{
      public int Id { get; set; }
      public int CustomerId { get; set; }
      public int ServiceId { get; set; }
      public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);
      public TimeSpan StartTime { get; set; }
      public BookingStatus Status { get; set; }
      public TimeSpan EndTime { get; set; }
      public decimal? TotalPrice { get; set; }
      public decimal? DiscountAmount { get; set; }
      public decimal? FinalPrice { get; set; }
      public string? Notes { get; set; }
}

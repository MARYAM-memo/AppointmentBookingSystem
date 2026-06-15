using AppointmentBooking.Application.ViewModels.Customer;
using AppointmentBooking.Application.ViewModels.Service;
using AppointmentBooking.Core.Models;

namespace AppointmentBooking.Application.ViewModels.Appointment;

public class AppointmentViewModel
{
      public int Id { get; set; }
      public DateTime AppointmentDate { get; set; }
      public TimeSpan StartTime { get; set; }
      public TimeSpan EndTime { get; set; }
      public BookingStatus Status { get; set; } = BookingStatus.Pending;
      public decimal? TotalPrice { get; set; }
      public decimal? DiscountAmount { get; set; }
      public decimal? FinalPrice { get; set; }
      public string? Currency { get; set; }
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? UpdatedAt { get; set; }
      public string? CreatedByUserId { get; set; }
      public bool IsDeleted { get; set; }
      public DateTime? DeletedAt { get; set; }
      public string? Notes { get; set; }
      public int ServiceId { get; set; }
      public ServiceResponseViewModel? Service { get; set; }
      public int CustomerId { get; set; }
      public CustomerViewModel? Customer { get; set; }
}

public class AppointmentCalendarViewModel
{
      public DateTime CurrentMonth { get; set; }
      public List<AppointmentViewModel> Appointments { get; set; } = [];
}
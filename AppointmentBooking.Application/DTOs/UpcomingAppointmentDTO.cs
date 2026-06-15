namespace AppointmentBooking.Application.DTOs;

public class UpcomingAppointmentDTO
{
      public int Id { get; set; }
      public string CustomerName { get; set; } = string.Empty;
      public string ServiceName { get; set; } = string.Empty;
      public TimeSpan StartTime { get; set; }
      public TimeSpan EndTime { get; set; }
      public string Status { get; set; } = string.Empty;
      public string? PhoneNumber { get; set; }
}

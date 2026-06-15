namespace AppointmentBooking.Application.DTOs;

public class TimeSlotDTO
{
      public TimeSpan StartTime { get; set; }
      public TimeSpan EndTime { get; set; }
      public bool IsAvailable { get; set; }
      public bool IsPastSlot { get; set; }
      public bool IsCurrentAppointment { get; set; }
      public int BookedCount { get; set; }
      public int MaxCapacity { get; set; }
      public string DisplayText { get; set; } = string.Empty;
}

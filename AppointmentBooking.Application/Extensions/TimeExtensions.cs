namespace AppointmentBooking.Application.Extensions;

public static class TimeExtensions
{
      public static string FormatTime(this TimeSpan input)
      {
            return input.ToString(@"hh\:mm");
      }
}

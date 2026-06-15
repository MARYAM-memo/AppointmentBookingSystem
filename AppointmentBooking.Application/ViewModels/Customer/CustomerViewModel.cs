namespace AppointmentBooking.Application.ViewModels.Customer;

public class CustomerViewModel
{
      public int Id { get; set; }
      public string FullName { get; set; } = string.Empty;
      public string PhoneNumber { get; set; } = string.Empty;
      public string? Email { get; set; }
      public string? Notes { get; set; }
      public int TotalAppointments { get; set; }
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? UpdatedAt { get; set; }
      public DateTime? LastAppointmentDate { get; set; }
      public string JoinDate
      {
            get
            {
                  var duration = DateTime.UtcNow - CreatedAt;

                  if (duration.TotalDays >= 365)
                        return $"منذ {(int)(duration.TotalDays / 365)} سنة";

                  if (duration.TotalDays >= 30)
                        return $"منذ {(int)(duration.TotalDays / 30)} شهر";

                  if (duration.TotalDays >= 1)
                        return $"منذ {(int)duration.TotalDays} يوم";

                  if (duration.TotalHours >= 1)
                        return $"منذ {(int)duration.TotalHours} ساعة";

                  if (duration.TotalMinutes >= 1)
                        return $"منذ {(int)duration.TotalMinutes} دقيقة";

                  return "منذ لحظات";
            }
      }
}

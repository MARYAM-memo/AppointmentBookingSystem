namespace AppointmentBooking.Application.Settings;

public class RateLimitingSettings
{
      public Dictionary<string, RateLimitPolicy> Policies { get; set; } = new();
}

public class RateLimitPolicy
{
      public int PermitLimit { get; set; }
      public int WindowMinutes { get; set; }
      public int QueueLimit { get; set; }
}

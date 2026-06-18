namespace AppointmentBooking.Application.Settings;

public class IdentitySettings
{
      public PasswordSettings Password { get; set; } = new();
}

public class PasswordSettings
{
      public bool RequireDigit { get; set; } = true;
      public int RequiredLength { get; set; } = 6;
      public int RequiredMaxLength { get; set; } = 50;
      public bool RequireNonAlphanumeric { get; set; } = true;
      public bool RequireUppercase { get; set; } = true;
      public bool RequireLowercase { get; set; } = true;
}
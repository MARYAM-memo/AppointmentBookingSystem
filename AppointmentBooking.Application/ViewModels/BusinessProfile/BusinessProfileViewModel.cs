using AppointmentBooking.Application.Shared;

namespace AppointmentBooking.Application.ViewModels.BusinessProfile;

public class BusinessProfileViewModel
{
      public string BusinessName { get; set; } = string.Empty;
      public string BusinessType { get; set; } = string.Empty;
      public string? Tagline { get; set; }
      public string? LogoUrl { get; set; }
      public string? FaviconUrl { get; set; }

      public BrandingColorsViewModel Colors { get; set; } = new();
      public LocalizationConfigViewModel Localization { get; set; } = new();
      public Dictionary<string, string> CustomLabels { get; set; } = [];
      public ContactInfoViewModel Contact { get; set; } = new();

      public TimeSpan WorkingHoursStart { get; set; }
      public TimeSpan WorkingHoursEnd { get; set; }
      public int SlotDurationMinutes { get; set; }

      // Helper properties
      public string Lang => Methods.GetLanguage(Localization.Language);
      public string Dir => Methods.GetDirection(Localization.Language) ?? Localization?.Direction ?? "";
      public bool IsRtl => Dir == "rtl";
}

public class BrandingColorsViewModel
{
      public string Primary { get; set; } = string.Empty;
      public string Secondary { get; set; } = string.Empty;
      public string Accent { get; set; } = string.Empty;
      public string? DarkModePrimary { get; set; }
}

public class LocalizationConfigViewModel
{
      public string Currency { get; set; } = string.Empty;
      public string Language { get; set; } = string.Empty;
      public string Direction { get; set; } = string.Empty;
      public string TimeZone { get; set; } = string.Empty;
}

public class ContactInfoViewModel
{
      public string? Phone { get; set; }
      public string? Email { get; set; }
      public string? Address { get; set; }
}

public class WorkingHoursViewModel
{
      public TimeSpan Start { get; set; }
      public TimeSpan End { get; set; }
      public int SlotDuration { get; set; }
}

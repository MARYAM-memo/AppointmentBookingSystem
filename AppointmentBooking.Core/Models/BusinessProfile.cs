using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppointmentBooking.Core.Abstractions;

namespace AppointmentBooking.Core.Models;

public class BusinessProfile : IEntity
{
      [Key]
      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public int Id { get; set; }

      // Visual identity
      public string BusinessName { get; set; } = string.Empty;
      public string BusinessType { get; set; } = string.Empty; // Salon, Clinic, Consultant, etc.
      public string? Tagline { get; set; }
      public string? LogoUrl { get; set; }
      public string? FaviconUrl { get; set; }

      // Colors
      public BrandingColors Colors { get; set; } = new();

      //General settings
      public LocalizationConfig Localization { get; set; } = new();

      //Working hours
      public TimeSpan WorkingHoursStart { get; set; }
      public TimeSpan WorkingHoursEnd { get; set; }
      public int SlotDurationMinutes { get; set; }

      // Custom Labels (Labels by Business Type)
      public Dictionary<string, string> CustomLabels { get; set; } = [];

      // Contact
      public ContactInfo Contact { get; set; } = new();

      // Audit
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? UpdatedAt { get; set; }
}


// Configurations
public class BrandingColors
{
      public string Primary { get; set; } = string.Empty;
      public string Secondary { get; set; } = string.Empty;
      public string Accent { get; set; } = string.Empty;
      public string? DarkModePrimary { get; set; }
}

public class LocalizationConfig
{
      public string Currency { get; set; } = string.Empty;
      public string Language { get; set; } = string.Empty;
      public string Direction { get; set; } = string.Empty;
}

public class ContactInfo
{
      public string? Phone { get; set; }
      public string? Email { get; set; }
      public string? Address { get; set; }
}




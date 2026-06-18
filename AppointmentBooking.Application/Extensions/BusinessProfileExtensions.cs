using System.Globalization;
using AppointmentBooking.Application.ViewModels.BusinessProfile;
using AppointmentBooking.Application.Shared;
using AppointmentBooking.Core.Models;
using Microsoft.Extensions.Localization;

namespace AppointmentBooking.Application.Extensions;

public static class BusinessProfileExtensions
{
      /// <summary>
      /// Returns a CultureInfo object based on the business profile's language setting (converted via CultureSwitch).
      /// </summary>
      public static CultureInfo GetCultureInfo(this BusinessProfileViewModel profile)
      {
            return new CultureInfo(Methods.CultureSwitch(profile.Localization.Language));
      }

      private static readonly HashSet<string> DefaultValues = new(StringComparer.OrdinalIgnoreCase)
      {
          // Arabic (from Constants)
          "الخدمات", "خدمة", "العملاء", "الحجوزات",
          // English
          "Services", "Service", "Customers", "Appointments",
          // Add more languages as needed...
      };

      /// <summary>
      /// Retrieves a navigation label by checking custom labels first, falling back to localized string, and defaulting to English if custom label equals default English value.
      /// </summary>
      public static string GetLabel(this Dictionary<string, string> labels, string key, LocalizedString localizedFallback)
      {
            var custom = labels?.GetValueOrDefault(key);
            var localized = localizedFallback.Value;

            if (string.IsNullOrWhiteSpace(custom) || DefaultValues.Contains(custom))
                  return localized;
            
            return custom;
      }
}

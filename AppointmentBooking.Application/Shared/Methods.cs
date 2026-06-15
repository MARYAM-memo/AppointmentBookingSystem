using System.Globalization;
using System.Text.RegularExpressions;

namespace AppointmentBooking.Application.Shared;

public class Methods
{
      /// <summary>
      /// Converts a hex color code to an RGB string format (returns default fallback color if conversion fails).
      /// </summary>
      public static string HexToRgb(string hex)
      {
            if (string.IsNullOrEmpty(hex) || hex.Length < 7) return "44, 110, 124";
            try
            {
                  var r = Convert.ToInt32(hex.Substring(1, 2), 16);
                  var g = Convert.ToInt32(hex.Substring(3, 2), 16);
                  var b = Convert.ToInt32(hex.Substring(5, 2), 16);
                  return $"{r}, {g}, {b}";
            }

            catch { return "44, 110, 124"; }
      }

      // Helper Methods
      /// <summary>
      /// Validates that the URL is either empty/null or a well-formed absolute HTTP/HTTPS URL.
      /// </summary>
      public static bool BeValidUrl(string? url)
      {
            if (string.IsNullOrEmpty(url)) return true;

            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
      }

      /// <summary>
      /// Checks if the given minutes value is divisible by 5 (e.g., for appointment slot durations).
      /// </summary>
      public static bool BeDivisibleBy5(int minutes)
      {
            return minutes % 5 == 0;
      }

      public static string RoundBy2(double input)
      {
            return input.ToString("0.##", CultureInfo.InvariantCulture);
      }
      public static string RoundBy2(decimal input)
      {
            return input.ToString("0.##", CultureInfo.InvariantCulture);
      }

      /// <summary>
      /// Converts a two-letter language code to a full culture code (ar -> ar-EG, en -> en-US, default ar-EG).
      /// </summary>
      public static string CultureSwitch(string culture)
      {
            return culture switch
            {
                  "ar" => "ar-EG",
                  "en" => "en-US",
                  _ => "ar-EG"
            };
      }

      /// <summary>
      /// Returns the two-letter language code ('ar' for Arabic, 'en' for English) based on input, defaulting to 'ar' if null.
      /// </summary>
      public static string GetLanguage(string? lang)
      {
            if (lang == null) return "ar";
            return lang == "ar" ? "ar" : "en";
      }

      /// <summary>
      /// Returns the text direction ('rtl' for Arabic, 'ltr' for English) based on language code, or null if input is null.
      /// </summary>
      public static string? GetDirection(string? lang)
      {
            if (lang == null) return null;
            return lang == "ar" ? "rtl" : "ltr";
      }

      /// <summary>
      /// Returns the appropriate font family CSS string for Arabic (Tajawal) or English (Inter) languages.
      /// </summary>
      public static string GetFontFamily(string lang)
      {
            return lang == "ar" ? "'Tajawal', 'Segoe UI', sans-serif" : "'Inter', 'Segoe UI', sans-serif";
      }

      /// <summary>
      /// Escapes special characters (%, _, [, ], \) in a string for use in SQL LIKE clauses to prevent pattern matching issues.
      /// </summary>
      public static string EscapeForLike(string input)
      {
            if (string.IsNullOrEmpty(input)) return input;

            // Escape special characters: % _ [ ] \
            return input
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_")
                .Replace("[", "\\[")
                .Replace("]", "\\]");
      }

      
}

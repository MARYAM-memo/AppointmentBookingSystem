using AppointmentBooking.Application.Shared;

namespace AppointmentBooking.Application.Settings;

public class LocalizationSettings
{
      public Dictionary<string, string> SupportedCultures { get; set; } = [];
      public string DefaultCulture { get; set; } = Constants.DefaultCulture;
      public string DefaultLanguage { get; set; } = Constants.DefaultLanguage;
      public List<string> AvailableLanguages { get; set; } = [];
      public List<string> SupportedCurrencies { get; set; } = [];
      public string DefaultCurrency { get; set; } = "ج.م";
}

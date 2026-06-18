using System.Globalization;
using AppointmentBooking.Application.Settings;
using AppointmentBooking.Application.Shared;

namespace AppointmentBooking.Application.Extensions;

public static class PriceExtension
{
      private static string _currentCurrency = Constants.DefaultCurrency;

      public static string CurrentCurrency
      {
            get => _currentCurrency;
            set
            {
                  if (!string.IsNullOrEmpty(value))
                  {
                        _currentCurrency = value;
                  }
            }
      }

      /// <summary>
      /// Convert price to text with currency
      /// </summary>
      public static string ToCurrency(this decimal price, string? currencyCode = null)
      {
            var currency = string.IsNullOrEmpty(currencyCode)
            ? CurrentCurrency
            : currencyCode;

            return $"{price.ToString("N2", CultureInfo.InvariantCulture)} {currency}";
      }

      /// <summary>
      /// for nullable
      /// </summary>
      public static string ToCurrency(this decimal? price, string? currencyCode = null)
      {
            if (!price.HasValue) return "-";
            var currency = string.IsNullOrEmpty(currencyCode)
            ? CurrentCurrency
            : currencyCode;

            return $"{price.Value.ToString("N2", CultureInfo.InvariantCulture)} {currency}";
      }

      public static string GetDefaultCurrency(LocalizationSettings settings)
      {
            return settings?.DefaultCurrency ?? Constants.DefaultCurrency;
      }

      public static List<string> GetSupportedCurrencies(this LocalizationSettings settings)
      {
            return settings?.SupportedCurrencies ?? [Constants.DefaultCurrency, "EGP"];
      }
}
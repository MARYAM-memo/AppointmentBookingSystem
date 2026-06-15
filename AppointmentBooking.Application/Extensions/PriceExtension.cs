using System.Globalization;
using AppointmentBooking.Application.Shared;

namespace AppointmentBooking.Application.Extensions;

public static class PriceExtension
{
      public static string CurrentCurrency { get; set; } = Constants.DefaultCurrency;

      /// <summary>
      /// Convert price to text with currency
      /// </summary>
      public static string ToCurrency(this decimal price)
      {
            return $"{price.ToString("N2", CultureInfo.InvariantCulture)} {CurrentCurrency}";
      }

      /// <summary>
      /// for nullable
      /// </summary>
      public static string ToCurrency(this decimal? price)
      {
            if (!price.HasValue) return "-";
            return $"{price.Value.ToString("N2", CultureInfo.InvariantCulture)} {CurrentCurrency}";
      }

}
using System.Globalization;
using AppointmentBooking.Application.ViewModels.BusinessProfile;
using AppointmentBooking.Application.Shared;

namespace AppointmentBooking.Application.Extensions;

public static class BusinessProfileExtensions
{
      public static CultureInfo GetCultureInfo(this BusinessProfileViewModel profile)
      {
            return new CultureInfo(Methods.CultureSwitch(profile.Localization.Language));
      }
}

using System.Globalization;
using AppointmentBooking.Application.Extensions;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.Shared;
using AppointmentBooking.Web.Resources.Shared;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;

namespace AppointmentBooking.Web.Middleware;

public class LocalizationMiddleware(RequestDelegate next)
{
      private readonly RequestDelegate _next = next;

      public async Task InvokeAsync(HttpContext context, IBusinessProfileService profileService, IStringLocalizer<SharedResource> localizer)
      {
            // Get language from cookie (user preference)
            var cookieLanguage = context.Request.Cookies[Constants.Keys.CookieUserLanguage];
            string culture;

            if (!string.IsNullOrEmpty(cookieLanguage))
            {
                  culture = Methods.CultureSwitch(cookieLanguage);
            }
            else
            {
                  // Fallback to database profile
                  var profile = await profileService.GetCurrentAsync();
                  culture = Methods.CultureSwitch(profile?.Localization?.Language ?? "");
            }

            var cultureInfo = new CultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            context.Response.Headers.Append("X-Content-Language", culture);
            context.Response.Headers.Append("Content-Language", culture);

            context.Items[Constants.Keys.ItemsHtmlLang] = culture;

            // IMPORTANT: Initialize the localizer
            BookingStatusExtensions.InitializeLocalizer(localizer);

            await _next(context);
      }
}

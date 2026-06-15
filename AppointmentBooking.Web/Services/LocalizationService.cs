using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Web.Resources.Validations;
using AppointmentBooking.Web.Resources.Views;
using Microsoft.Extensions.Localization;

namespace AppointmentBooking.Web.Services;

public class ValidationLocalizer(IStringLocalizer<ValidationsSharedResource> localizer) : ILocalizationService
{
      private readonly IStringLocalizer<ValidationsSharedResource> _localizer = localizer;
      public string this[string key] => _localizer[key];
      public string GetString(string key) => _localizer[key];
      public string GetString(string key, params object[] args) => string.Format(_localizer[key], args);
}
using AppointmentBooking.Application.Extensions;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.Settings;
using AppointmentBooking.Application.Shared;
using AppointmentBooking.Application.ViewModels.BusinessProfile;
using AppointmentBooking.Core.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AppointmentBooking.Web.Controllers
{
    [Authorize]
    public class BusinessProfileController(ILogger<BusinessProfileController> logger, IConfiguration configuration, IOptions<LocalizationSettings> localizationSettings, IFormPreparationService formPreparation) : BaseController
    {
        readonly ILogger<BusinessProfileController> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly LocalizationSettings _localizationSettings = localizationSettings.Value;
        private readonly IFormPreparationService _formPreparation= formPreparation;

        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            var profile = await GetCurrentProfileAsync();
            var model = Mapper.Map<BusinessProfileViewModel>(profile);
            return View(model);

        }

        [Authorize(Roles = Constants.AdminRole)]
        public async Task<ActionResult> Edit()
        {
            var profile = await GetCurrentProfileAsync();
            ViewBag.SupportedCurrencies= _localizationSettings.GetSupportedCurrencies();
            var model = Mapper.Map<BusinessProfileViewModel>(profile);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BusinessProfileViewModel model)
        {
            return await ValidateAndExecuteAsync<BusinessProfileViewModel, IValidator<BusinessProfileViewModel>>(
                model, nameof(Edit), async () =>
                {
                    var profile = await GetCurrentProfileAsync();

                    // Check if language changed
                    var oldLang = profile.Localization?.Language;
                    var newLang = model.Localization?.Language;
                    var languageChanged = oldLang != newLang && !string.IsNullOrEmpty(newLang);

                    var oldCurrency = profile.Localization?.Currency??"";
                    var newCurrency = model.Localization?.Currency??"";
                    var currencyChanged = oldCurrency != newCurrency && !string.IsNullOrEmpty(newCurrency);

                    // Validate currency is supported
                    if (currencyChanged && !_localizationSettings.GetSupportedCurrencies().Contains(newCurrency))
                    {
                        System.Console.WriteLine("Currency Changed && New is not supported");
                        WarningMessage(string.Format(Localizer["Warning_CurrencyNotSupported"], newCurrency, oldCurrency));
                        model.Localization?.Currency = oldCurrency;
                        newCurrency = oldCurrency;
                        currencyChanged = false;
                    }

                    Mapper.Map(model, profile);
                    await ProfileService.UpdateAsync(profile);

                    if (currencyChanged || languageChanged)
                    {
                        System.Console.WriteLine("Currency Changed || Language Changed");
                        // Invalidate all appointments caches
                        await _formPreparation.InvalidateCacheAsync();

                        if (currencyChanged)
                        {
                            System.Console.WriteLine("Currency Changed");
                            PriceExtension.CurrentCurrency = newCurrency;
                            ViewBag.Currency = newCurrency;
                        }
                    }

                    // If language changed → redirect to SetLanguage then back here
                    if (languageChanged)
                    {
                        System.Console.WriteLine("Language Changed");
                        var returnUrl = Url.Action(nameof(Edit));
                        return RedirectToAction("SetLanguage", "Account", new
                        {
                            culture = newLang,
                            returnUrl
                        });
                    }
                    
                    SuccessMessage(Localizer["BusinessProfile_UpdateSuccess"]);
                    return RedirectToAction(nameof(Index));
                }
            );
        }

        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<ActionResult> UpdateColorsAsync([FromBody] BrandingColorsViewModel colors)
        {
            var profile = await GetCurrentProfileAsync();
            profile.Colors = Mapper.Map<BrandingColors>(colors);
            profile.UpdatedAt = DateTime.UtcNow;
            await ProfileService.UpdateAsync(profile);

            SuccessMessage(Localizer["BusinessProfile_ColorsUpdateSuccess"]);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<ActionResult> UpdateLabelsAsync([FromBody] Dictionary<string, string> labels)
        {
            var profile = await GetCurrentProfileAsync();
            profile.CustomLabels = labels;
            profile.UpdatedAt = DateTime.UtcNow;
            await ProfileService.UpdateAsync(profile);

            SuccessMessage(Localizer["BusinessProfile_LabelsUpdateSuccess"]);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<ActionResult> UpdateWorkingHoursAsync([FromBody] WorkingHoursViewModel hours)
        {
            var profile = await GetCurrentProfileAsync();
            profile.WorkingHoursStart = hours.Start;
            profile.WorkingHoursEnd = hours.End;
            profile.SlotDurationMinutes = hours.SlotDuration;
            profile.UpdatedAt = DateTime.UtcNow;
            await ProfileService.UpdateAsync(profile);

            SuccessMessage(Localizer["BusinessProfile_WorkingHoursUpdateSuccess"]);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<ActionResult> ResetToDefaults()
        {
            var profile = await GetCurrentProfileAsync();

            var businessSettingsSection = _configuration.GetSection("BusinessSettings:DefaultProfile");
            var colorSection = businessSettingsSection.GetSection("Colors");

            // Reset all to defaults
            profile.BusinessName = businessSettingsSection["BusinessName"] ?? Localizer["BusinessProfile_DefaultBusinessName"];
            profile.BusinessType = businessSettingsSection["BusinessType"] ?? Localizer["BusinessProfile_DefaultBusinessType"];
            profile.Tagline = null;
            profile.LogoUrl = null;
            profile.FaviconUrl = null;
            profile.Colors = new BrandingColors
            {
                Primary = colorSection["Primary"] ?? Constants.DefaultPrimaryColor,
                Secondary = colorSection["Secondary"] ?? Constants.DefaultSecondaryColor,
                Accent = colorSection["Accent"] ?? Constants.DefaultAccentColor,
            };
            profile.Localization = new LocalizationConfig
            {
                Currency = businessSettingsSection["Currency"] ?? Constants.DefaultCurrency,
                Language = businessSettingsSection["Language"] ?? Constants.DefaultLanguage,
                Direction = businessSettingsSection["Direction"] ?? Constants.DefaultDirection,
            };
            profile.WorkingHoursStart = TimeSpan.TryParse(businessSettingsSection["WorkingHoursStart"], out var workingHoursStart) ? workingHoursStart : TimeSpan.FromHours(Constants.Defaults.workingHoursStart);
            profile.WorkingHoursEnd = TimeSpan.TryParse(businessSettingsSection["WorkingHoursEnd"], out var workingHoursEnd) ? workingHoursEnd : TimeSpan.FromHours(Constants.Defaults.workingHoursEnd);
            profile.SlotDurationMinutes = int.TryParse(businessSettingsSection["SlotDurationMinutes"], out var slotDuration) ? slotDuration : Constants.Defaults.SlotDurationMinutes;
            profile.Contact = new ContactInfo();
            profile.CustomLabels = businessSettingsSection.GetSection("CustomLabels").Get<Dictionary<string, string>>() ?? new()
                {
                    { "service", Localizer["BusinessProfile_DefaultLabel_Service"] },
                    { "serviceItem", Localizer["BusinessProfile_DefaultLabel_ServiceItem"] },
                    { "customer", Localizer["BusinessProfile_DefaultLabel_Customer"] },
                    { "appointment", Localizer["BusinessProfile_DefaultLabel_Booking"] }
                };
            profile.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Resetting profile. BusinessName: {BusinessName}", profile.BusinessName);
            await ProfileService.UpdateAsync(profile);

            PriceExtension.CurrentCurrency = profile.Localization.Currency;

            SuccessMessage(Localizer["BusinessProfile_ResetToDefaultsSuccess"]);
            return RedirectToAction(nameof(Index));
        }
    }
}

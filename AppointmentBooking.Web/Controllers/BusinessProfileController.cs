using AppointmentBooking.Application.Extensions;
using AppointmentBooking.Application.Shared;
using AppointmentBooking.Application.ViewModels.BusinessProfile;
using AppointmentBooking.Core.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentBooking.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BusinessProfileController(ILogger<BusinessProfileController> logger, IConfiguration configuration) : BaseController
    {
        readonly ILogger<BusinessProfileController> _logger = logger;
        private readonly IConfiguration _configuration=configuration;

        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            var profile = await GetCurrentProfileAsync();
            var model = Mapper.Map<BusinessProfileViewModel>(profile);
            return View(model);

        }

        public async Task<ActionResult> Edit()
        {
            var profile = await GetCurrentProfileAsync();
            var model = Mapper.Map<BusinessProfileViewModel>(profile);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BusinessProfileViewModel model)
        {
            return await ValidateAndExecuteAsync<BusinessProfileViewModel, IValidator<BusinessProfileViewModel>>(
                model, nameof(Edit), async () =>
                {
                    var profile = await GetCurrentProfileAsync();
                    Mapper.Map(model, profile);
                    await ProfileService.UpdateAsync(profile);

                    PriceExtension.CurrentCurrency = profile.Localization.Currency;
                    SuccessMessage(Localizer["BusinessProfile_UpdateSuccess"]);
                    return RedirectToAction(nameof(Index));
                }
            );
        }

        [HttpPost]
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
                TimeZone = businessSettingsSection["TimeZone"] ?? Constants.DefaultTimeZone,

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

        [AllowAnonymous]
        public async Task<ActionResult> PreviewAsync()
        {
            var profile = await GetCurrentProfileAsync();
            return View(profile);
        }
    }
}

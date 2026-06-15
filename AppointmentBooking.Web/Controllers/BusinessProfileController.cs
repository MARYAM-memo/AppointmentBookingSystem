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
    public class BusinessProfileController(ILogger<BusinessProfileController> logger) : BaseController
    {
        readonly ILogger<BusinessProfileController> _logger = logger;

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
            // Reset all to defaults
            profile.BusinessName = Localizer["BusinessProfile_DefaultBusinessName"];
            profile.BusinessType = Localizer["BusinessProfile_DefaultBusinessType"];
            profile.Tagline = null;
            profile.LogoUrl = null;
            profile.FaviconUrl = null;
            profile.Colors = new BrandingColors();
            profile.Localization = new LocalizationConfig();
            profile.WorkingHoursStart = TimeSpan.FromHours(Constants.Defaults.workingHoursStart);
            profile.WorkingHoursEnd = TimeSpan.FromHours(Constants.Defaults.workingHoursEnd);
            profile.SlotDurationMinutes = Constants.Defaults.SlotDurationMinutes;
            profile.Contact = new ContactInfo();
            profile.CustomLabels = new Dictionary<string, string>
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

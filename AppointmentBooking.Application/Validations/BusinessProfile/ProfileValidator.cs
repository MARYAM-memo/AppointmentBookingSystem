using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.BusinessProfile;
using AppointmentBooking.Application.Shared;
using FluentValidation;

namespace AppointmentBooking.Application.Validations.BusinessProfile;

public class ProfileValidator : AbstractValidator<BusinessProfileViewModel>
{
    private readonly ILocalizationService _localizer;

    public ProfileValidator(
        ILocalizationService localizer,
        BrandingColorsValidator colorsValidator,
        LocalizationConfigValidator localizationValidator,
        ContactInfoValidator contactValidator)
    {
        _localizer = localizer;

        // Checking the business name
        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage(_localizer["Profile_BusinessName_Required"])
            .MinimumLength(3).WithMessage(_localizer["Profile_BusinessName_MinLength"])
            .MaximumLength(100).WithMessage(_localizer["Profile_BusinessName_MaxLength"])
            .Matches(@"^[\u0600-\u06FFa-zA-Z0-9\s\-_]+$")
            .WithMessage(_localizer["Profile_BusinessName_InvalidPattern"]);

        // Check Tagline
        RuleFor(x => x.Tagline)
            .MaximumLength(200).WithMessage(_localizer["Profile_Tagline_MaxLength"])
            .When(x => !string.IsNullOrEmpty(x.Tagline));

        // URLs verification
        RuleFor(x => x.LogoUrl)
            .Must(Methods.BeValidUrl).WithMessage(_localizer["Profile_LogoUrl_Invalid"])
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));

        RuleFor(x => x.FaviconUrl)
            .Must(Methods.BeValidUrl).WithMessage(_localizer["Profile_FaviconUrl_Invalid"])
            .When(x => !string.IsNullOrEmpty(x.FaviconUrl));

        // Checking working hours
        RuleFor(x => x.WorkingHoursStart)
            .NotNull().WithMessage(_localizer["Profile_WorkingHoursStart_Required"]);

        RuleFor(x => x.WorkingHoursEnd)
            .NotNull().WithMessage(_localizer["Profile_WorkingHoursEnd_Required"])
            .GreaterThan(x => x.WorkingHoursStart)
            .WithMessage(_localizer["Profile_WorkingHoursEnd_AfterStart"]);

        // Check the duration of the time slot
        RuleFor(x => x.SlotDurationMinutes)
            .InclusiveBetween(5, 120)
            .WithMessage(_localizer["Profile_SlotDuration_InclusiveBetween"])
            .Must(Methods.BeDivisibleBy5)
            .WithMessage(_localizer["Profile_SlotDuration_DivisibleBy5"]);

        // Checking for nested objects

        RuleFor(x => x.Colors)
            .NotNull().WithMessage(_localizer["Profile_Colors_Required"])
            .SetValidator(new BrandingColorsValidator(_localizer));

        RuleFor(x => x.Localization)
            .NotNull().WithMessage(_localizer["Profile_Localization_Required"])
            .SetValidator(new LocalizationConfigValidator(_localizer));

        RuleFor(x => x.Contact)
             .NotNull().WithMessage(_localizer["Profile_Contact_Required"])
            .SetValidator(new ContactInfoValidator(_localizer));

        // Check Custom Labels
        RuleFor(x => x.CustomLabels)
            .Must(labels => labels == null || labels.Count <= 20)
            .WithMessage(_localizer["Profile_CustomLabels_MaxCount"])
            .Must(labels => labels == null || labels.Count == 0 ||
                labels.All(kvp => !string.IsNullOrWhiteSpace(kvp.Key) &&
                                 !string.IsNullOrWhiteSpace(kvp.Value)))
            .WithMessage(_localizer["Profile_CustomLabels_NotEmpty"]);

        // Advanced rules
        RuleFor(x => x)
            .Must(HaveLogicalWorkingHours)
            .WithMessage(_localizer["Profile_WorkingHours_Logical"]);

        RuleFor(x => x)
            .Must(HaveValidLogoAndFavicon)
            .WithMessage(_localizer["Profile_LogoAndFavicon_SameDomain"]);
    }

    /// <summary>
    /// Ensures the working hours duration meets the minimum required logical working hours per day (e.g., at least 4 hours).
    /// </summary>
    private bool HaveLogicalWorkingHours(BusinessProfileViewModel model)
    {
        var start = model.WorkingHoursStart;
        var end = model.WorkingHoursEnd;
        var duration = end - start;

        return duration.TotalHours >= Constants.Defaults.MinLogicalWorkingHours; // At least 4 working hours per day
    }

    /// <summary>
    /// Validates that if both logo and favicon URLs are provided, they belong to the same host domain.
    /// </summary>
    private bool HaveValidLogoAndFavicon(BusinessProfileViewModel model)
    {
        if (string.IsNullOrEmpty(model.LogoUrl) || string.IsNullOrEmpty(model.FaviconUrl))
            return true;

        try
        {
            var logoUri = new Uri(model.LogoUrl);
            var faviconUri = new Uri(model.FaviconUrl);
            return logoUri.Host == faviconUri.Host;
        }
        catch
        {
            return false;
        }
    }
}
using System.Text.RegularExpressions;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.BusinessProfile;
using FluentValidation;

namespace AppointmentBooking.Application.Validations.BusinessProfile;

public partial class BrandingColorsValidator : AbstractValidator<BrandingColorsViewModel>
{
    private readonly ILocalizationService _localizer;

    public BrandingColorsValidator(ILocalizationService localizer)
    {
        _localizer = localizer;

        // Checking the colors

        RuleFor(x => x.Primary)
            .NotEmpty().WithMessage(_localizer["Branding_Primary_Required"])
            .Must(BeValidHexColor).WithMessage(_localizer["Branding_Primary_InvalidHex"]);

        RuleFor(x => x.Secondary)
            .NotEmpty().WithMessage(_localizer["Branding_Secondary_Required"])
            .Must(BeValidHexColor).WithMessage(_localizer["Branding_Secondary_InvalidHex"]);

        RuleFor(x => x.Accent)
            .NotEmpty().WithMessage(_localizer["Branding_Accent_Required"])
            .Must(BeValidHexColor).WithMessage(_localizer["Branding_Accent_InvalidHex"]);

        // Check the color in dark mode (optional)
        RuleFor(x => x.DarkModePrimary)
            .Must(BeValidHexColor).WithMessage(_localizer["Branding_DarkModePrimary_InvalidHex"])
            .When(x => !string.IsNullOrEmpty(x.DarkModePrimary));

        // Checking for color contrast
        RuleFor(x => x)
            .Must(HaveGoodContrast)
            .WithMessage(_localizer["Branding_Contrast_Required"]);
    }
    /// <summary>
    /// Validates that the provided string is a valid hex color code (3 or 6 characters, preceded by #, using A-F or 0-9).
    /// </summary>
    private bool BeValidHexColor(string? color)
    {
        if (string.IsNullOrEmpty(color)) return false;

        var hexPattern = MyRegex();
        return hexPattern.IsMatch(color);
    }

    /// <summary>
    /// Ensures the primary and secondary branding colors are different to maintain visual contrast.
    /// </summary>
    private bool HaveGoodContrast(BrandingColorsViewModel colors)
    {
        return colors.Primary != colors.Secondary;
    }

    /// <summary>
    /// Regular expression pattern for validating hex color codes (supports both 3-digit and 6-digit formats).
    /// </summary>
    [GeneratedRegex(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")]
    private static partial Regex MyRegex();
}
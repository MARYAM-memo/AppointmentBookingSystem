using System.Globalization;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.BusinessProfile;
using FluentValidation;

namespace AppointmentBooking.Application.Validations.BusinessProfile;

public class LocalizationConfigValidator : AbstractValidator<LocalizationConfigViewModel>
{
      private readonly ILocalizationService _localizer;

      public LocalizationConfigValidator(ILocalizationService localizer)
      {
            _localizer = localizer;

            // Verify currency
            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage(_localizer["Localization_Currency_Required"])
                .MaximumLength(10).WithMessage(_localizer["Localization_Currency_MaxLength"]);

            // Check language
            RuleFor(x => x.Language)
                .NotEmpty().WithMessage(_localizer["Localization_Language_Required"])
                .Must(lang => lang == "ar" || lang == "en")
                .WithMessage(_localizer["Localization_Language_Invalid"]);

            // Check the UI dir
            RuleFor(x => x.Direction)
                .NotEmpty().WithMessage(_localizer["Localization_Direction_Required"])
                .Must(dir => dir == "rtl" || dir == "ltr")
                .WithMessage(_localizer["Localization_Direction_Invalid"]);

            // Advanced
            RuleFor(x => x)
                .Must(HaveConsistentLanguageAndDirection)
                .WithMessage(_localizer["Localization_ConsistentLanguageAndDirection"]);
      }

      /// <summary>
      /// Ensures the text direction (LTR/RTL) is consistent with the selected language (RTL for Arabic variants, LTR for others).
      /// </summary>
      private bool HaveConsistentLanguageAndDirection(LocalizationConfigViewModel config)
      {
            var arabicLanguages = new[] { "ar", "ar-EG", "ar-SA", "ar-AE", "ar-IQ" };
            var isArabic = arabicLanguages.Contains(config.Language);

            return isArabic ? config.Direction == "rtl" : config.Direction == "ltr";
      }
}
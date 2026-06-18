using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.Account;
using FluentValidation;

namespace AppointmentBooking.Application.Validations.Account;

public class RegisterValidation : AbstractValidator<RegisterViewModel>
{
    public RegisterValidation(ILocalizationService localizer)
    {
        // Check name
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["Name_Required"])
            .MinimumLength(3).WithMessage(localizer["Name_MinLength"])
            .MaximumLength(50).WithMessage(localizer["Name_MaxLength"])
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s]+$").WithMessage(localizer["Name_InvalidPattern"]);

        // Email verification
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(localizer["Email_Required"])
            .EmailAddress().WithMessage(localizer["Email_Invalid"])
            .MaximumLength(100).WithMessage(localizer["Email_MaxLength"]);

        // Password verification (with strong criteria)
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(localizer["Password_Required"]);

        // Checking for a match in the password confirmation
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(localizer["ConfirmPassword_Required"])
            .Equal(x => x.Password).WithMessage(localizer["ConfirmPassword_NotMatch"]);
    }
}

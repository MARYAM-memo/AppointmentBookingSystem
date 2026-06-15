using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.Account;
using FluentValidation;

namespace AppointmentBooking.Application.Validations.Account;

public class LoginValidation : AbstractValidator<LoginViewModel>
{

    public LoginValidation(ILocalizationService localizer)
    {
        // Email verification
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(localizer["Email_Required"])
            .EmailAddress().WithMessage(localizer["Email_Invalid"])
            .MaximumLength(100).WithMessage(localizer["Email_MaxLength"]);

        // Password verification
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(localizer["Password_Required"])
            .MinimumLength(6).WithMessage(localizer["Password_MinLength"])
            .MaximumLength(50).WithMessage(localizer["Password_MaxLength"]);

        // RememberMe verification
        RuleFor(x => x.RememberMe)
            .NotNull().WithMessage(localizer["RememberMe_Required"]);
    }
}

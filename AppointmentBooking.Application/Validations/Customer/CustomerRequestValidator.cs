using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.Customer;
using FluentValidation;

namespace AppointmentBooking.Application.Validations.Customer;

public partial class CustomerRequestValidator : AbstractValidator<CustomerRequestViewModel>
{
    private readonly ILocalizationService _localizer;

    public CustomerRequestValidator(ILocalizationService localizer)
    {
        _localizer = localizer;

        // Rule for Id (only for edit mode)
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .When(x => x.Id > 0)
            .WithMessage(_localizer["Customer_Id_Invalid"]);

        // Rule for FullName
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage(_localizer["Customer_FullName_Required"])
            .MinimumLength(3)
            .WithMessage(_localizer["Customer_FullName_MinLength"])
            .MaximumLength(100)
            .WithMessage(_localizer["Customer_FullName_MaxLength"])
            .Matches(@"^[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FFa-zA-Z\s\-\.]+$")
            .WithMessage(_localizer["Customer_FullName_InvalidPattern"]);

        // Rule for PhoneNumber
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage(_localizer["Customer_Phone_Required"])
            .MinimumLength(8)
            .WithMessage(_localizer["Customer_Phone_MinLength"])
            .MaximumLength(15)
            .WithMessage(_localizer["Customer_Phone_MaxLength"]);

        // Rule for Email
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(_localizer["Customer_Email_Invalid"])
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(_localizer["Customer_Email_MaxLength"]);

        // Rule for Notes
        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage(_localizer["Customer_Notes_MaxLength"])
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

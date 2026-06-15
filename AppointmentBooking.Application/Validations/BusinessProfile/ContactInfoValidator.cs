using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.BusinessProfile;
using FluentValidation;

namespace AppointmentBooking.Application.Validations.BusinessProfile;

public class ContactInfoValidator : AbstractValidator<ContactInfoViewModel>
{
    private readonly ILocalizationService _localizer;

    public ContactInfoValidator(ILocalizationService localizer)
    {
        _localizer = localizer;

        // Phone number verification
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage(_localizer["Contact_Phone_MaxLength"])
            .Matches(@"^[\+\d\s\-\(\)]+$").WithMessage(_localizer["Contact_Phone_InvalidFormat"])
            .When(x => !string.IsNullOrEmpty(x.Phone));

        // Email verification
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage(_localizer["Contact_Email_InvalidFormat"])
            .MaximumLength(100).WithMessage(_localizer["Contact_Email_MaxLength"])
            .When(x => !string.IsNullOrEmpty(x.Email));

        // Check the address
        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage(_localizer["Contact_Address_MaxLength"])
            .When(x => !string.IsNullOrEmpty(x.Address));

        // Ensure there is at least one method of communication
        RuleFor(x => x)
            .Must(HaveAtLeastOneContactMethod)
            .WithMessage(_localizer["Contact_AtLeastOneMethod"]);
    }

    /// <summary>
    /// Ensures at least one contact method (phone, email, or address) is provided.
    /// </summary>
    private bool HaveAtLeastOneContactMethod(ContactInfoViewModel contact)
    {
        return !string.IsNullOrEmpty(contact.Phone) ||
               !string.IsNullOrEmpty(contact.Email) ||
               !string.IsNullOrEmpty(contact.Address);
    }
}
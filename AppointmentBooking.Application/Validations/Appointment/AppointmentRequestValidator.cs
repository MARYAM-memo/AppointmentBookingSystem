using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.Appointment;
using FluentValidation;

namespace AppointmentBooking.Application.Validations.Appointment;

public class AppointmentRequestValidator : AbstractValidator<AppointmentRequestViewModel>
{
    readonly IBusinessProfileService _businessProfile;
    readonly ILocalizationService _localizer;
    public AppointmentRequestValidator(IBusinessProfileService businessProfile, ILocalizationService localizer)
    {
        _businessProfile = businessProfile;
        _localizer = localizer;

        // Rule for Id (only for edit mode)
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .When(x => x.Id > 0)
            .WithMessage(_localizer["Appointment_Id_Invalid"]);

        // Rule for CustomerId
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage(_localizer["Appointment_Customer_Required"]);

        // Rule for ServiceId
        RuleFor(x => x.ServiceId)
            .GreaterThan(0)
            .WithMessage(_localizer["Appointment_Service_Required"]);

        // Rule for AppointmentDate
        RuleFor(x => x.AppointmentDate)
            .NotEmpty()
            .WithMessage(_localizer["Appointment_Date_Required"])
            .Must(date => date.Date >= DateTime.Today)
            .WithMessage(_localizer["Appointment_Date_Past"])
            .Must(date => !IsWeekend(date))
            .WithMessage(_localizer["Appointment_Date_Weekend"]);

        // Rule for StartTime
        RuleFor(x => x.StartTime)
            .NotEmpty()
            .WithMessage(_localizer["Appointment_Time_Required"])
            .Must(BeValidBusinessHours)
            .WithMessage(_localizer["Appointment_Time_InvalidBusinessHours"])
            .Must(BeInFutureTime)
            .When(x => x.AppointmentDate.Date == DateTime.Today)
            .WithMessage(_localizer["Appointment_Time_Past"]);

        // Rule for Status
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage(_localizer["Appointment_Status_Invalid"]);

        // Rules for pricing
        RuleFor(x => x)
            .Must(BeValidPricing)
            .WithMessage(_localizer["Appointment_Pricing_Invalid"]);

        // Rule for Notes (optional with max length)
        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage(_localizer["Appointment_Notes_MaxLength"]);
    }

    /// <summary>
    /// Validates the pricing structure of an appointment request. Ensures total price is positive, discount and final price are non-negative and within bounds, and that the formula Total - Discount = Final holds within a 0.01 tolerance.
    /// </summary>
    private bool BeValidPricing(AppointmentRequestViewModel vm)
    {
        // 1. Check the total price
        if (vm.TotalPrice.HasValue && vm.TotalPrice.Value <= 0)
            return false;

        // 2. Check the discount value (not negative and not exceeding the total)
        if (vm.DiscountAmount.HasValue)
        {
            if (vm.DiscountAmount.Value < 0)
                return false;

            if (vm.TotalPrice.HasValue && vm.DiscountAmount.Value > vm.TotalPrice.Value)
                return false;
        }

        // 3. Verify the final price (not negative and not exceeding the total)
        if (vm.FinalPrice.HasValue)
        {
            if (vm.FinalPrice.Value < 0)
                return false;

            if (vm.TotalPrice.HasValue && vm.FinalPrice.Value > vm.TotalPrice.Value)
                return false;
        }

        // 4. Verify the calculation formula (TotalPrice - Discount = FinalPrice)
        if (vm.TotalPrice.HasValue && vm.DiscountAmount.HasValue && vm.FinalPrice.HasValue)
        {
            // Use rounding to compare decimal numbers if applicable
            decimal expectedFinalPrice = vm.TotalPrice.Value - vm.DiscountAmount.Value;
            if (Math.Abs(expectedFinalPrice - vm.FinalPrice.Value) > 0.01m) // Tolerance 0.01
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the given date falls on a weekend (Friday or Saturday).
    /// </summary>
    private static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday;
    }

    private bool BeValidBusinessHours(TimeSpan time)
    {
        try
        {
            var profile = _businessProfile.GetCurrentAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            return time >= profile.WorkingHoursStart && time <= profile.WorkingHoursEnd;
        }
        catch
        {
            return true; // If profile not available, skip validation
        }
    }
    /// <summary>
    /// Checks if the given time is in the future, allowing a 5-minute buffer for immediate appointments.
    /// </summary>
    private bool BeInFutureTime(TimeSpan time)
    {
        var now = DateTime.UtcNow;
        var currentTime = new TimeSpan(now.Hour, now.Minute, now.Second);

        // Allow 5 minutes buffer for immediate appointment
        return time > currentTime.Add(TimeSpan.FromMinutes(5));
    }
}

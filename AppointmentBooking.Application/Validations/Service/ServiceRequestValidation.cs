using System.Text.RegularExpressions;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.Service;
using AppointmentBooking.Application.Shared;
using FluentValidation;

namespace AppointmentBooking.Application.Validations.Service;

public partial class ServiceRequestValidation : AbstractValidator<ServiceRequestViewModel>
{
    private readonly ILocalizationService _localizer;

    public ServiceRequestValidation(ILocalizationService localizer)
    {
        _localizer = localizer;

        // Check Id (for updates)
        RuleFor(x => x.Id)
            .GreaterThanOrEqualTo(0)
            .WithMessage(_localizer["Service_Id_Invalid"]);

        // Service name verification
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_localizer["Service_Name_Required"])
            .MinimumLength(3).WithMessage(_localizer["Service_Name_MinLength"])
            .MaximumLength(200).WithMessage(_localizer["Service_Name_MaxLength"])
            .Matches(@"^[\u0600-\u06FFa-zA-Z0-9\s\-_،]+$")
            .WithMessage(_localizer["Service_Name_InvalidPattern"]);

        // Verify the description
        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage(_localizer["Service_Description_MaxLength"])
            .When(x => !string.IsNullOrEmpty(x.Description));

        // Check classification
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage(_localizer["Service_Category_Required"])
            .MaximumLength(100).WithMessage(_localizer["Service_Category_MaxLength"])
            .Matches(@"^[\u0600-\u06FFa-zA-Z0-9\s\-_]+$")
            .WithMessage(_localizer["Service_Category_InvalidPattern"]);

        // Check subcategory
        RuleFor(x => x.SubCategory)
            .MaximumLength(100)
            .WithMessage(_localizer["Service_SubCategory_MaxLength"])
            .When(x => !string.IsNullOrEmpty(x.SubCategory));

        // Check the duration in minutes
        RuleFor(x => x.DurationMinutes)
            .NotEmpty().WithMessage(_localizer["Service_Duration_Required"])
            .InclusiveBetween(5, 480)
            .WithMessage(_localizer["Service_Duration_InclusiveBetween"])
            .Must(Methods.BeDivisibleBy5)
            .WithMessage(_localizer["Service_Duration_DivisibleBy5"]);

        // Check the price
        RuleFor(x => x.Price)
            .NotEmpty().WithMessage(_localizer["Service_Price_Required"])
            .GreaterThanOrEqualTo(0).WithMessage(_localizer["Service_Price_Negative"])
            .LessThanOrEqualTo(100000).WithMessage(_localizer["Service_Price_MaxValue"]);

        // Check the price for the additional hour
        RuleFor(x => x.PricePerAdditionalHour)
            .GreaterThanOrEqualTo(0)
            .WithMessage(_localizer["Service_PricePerExtraHour_Negative"])
            .LessThanOrEqualTo(50000)
            .WithMessage(_localizer["Service_PricePerExtraHour_MaxValue"])
            .Must((model, pricePerHour) => pricePerHour == null || pricePerHour <= model.Price * 2)
            .WithMessage(_localizer["Service_PricePerExtraHour_NotExceedDouble"])
            .When(x => x.PricePerAdditionalHour.HasValue);

        // Check preparation time before service
        RuleFor(x => x.BufferBeforeMinutes)
            .InclusiveBetween(0, 120)
            .WithMessage(_localizer["Service_BufferBefore_InclusiveBetween"])
            .Must((model, buffer) => buffer == null || buffer <= model.DurationMinutes)
            .WithMessage(_localizer["Service_BufferBefore_NotExceedDuration"])
            .When(x => x.BufferBeforeMinutes.HasValue);

        // Checking the preparation time after the service
        RuleFor(x => x.BufferAfterMinutes)
            .InclusiveBetween(0, 120)
            .WithMessage(_localizer["Service_BufferAfter_InclusiveBetween"])
            .Must((model, buffer) => buffer == null || buffer <= model.DurationMinutes)
            .WithMessage(_localizer["Service_BufferAfter_NotExceedDuration"])
            .When(x => x.BufferAfterMinutes.HasValue);

        // Checking the maximum capacity
        RuleFor(x => x.MaxCapacity)
            .NotEmpty().WithMessage(_localizer["Service_MaxCapacity_Required"])
            .InclusiveBetween(1, 100)
            .WithMessage(_localizer["Service_MaxCapacity_InclusiveBetween"]);

        // Checking the required documents 
        RuleFor(x => x.RequiredDocuments)
            .MaximumLength(1000)
            .WithMessage(_localizer["Service_RequiredDocuments_MaxLength"])
            .When(x => !string.IsNullOrEmpty(x.RequiredDocuments));

        // Check the icon
        RuleFor(x => x.Icon)
            .Matches(@"^bi-[a-z0-9\-]+$")
            .WithMessage(_localizer["Service_Icon_InvalidPattern"])
            .MaximumLength(50)
            .WithMessage(_localizer["Service_Icon_MaxLength"])
            .When(x => !string.IsNullOrEmpty(x.Icon));

        // Check the color
        RuleFor(x => x.Color)
            .Must(BeValidHexColor)
            .WithMessage(_localizer["Service_Color_InvalidHex"])
            .When(x => !string.IsNullOrEmpty(x.Color));

        // Image link verification
        RuleFor(x => x.ImageUrl)
            .Must(Methods.BeValidUrl)
            .WithMessage(_localizer["Service_ImageUrl_Invalid"])
            .MaximumLength(500)
            .WithMessage(_localizer["Service_ImageUrl_MaxLength"])
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        // Checking the display order
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(_localizer["Service_DisplayOrder_Positive"])
            .LessThanOrEqualTo(1000)
            .WithMessage(_localizer["Service_DisplayOrder_MaxValue"]);

        // Advanced rules
        RuleFor(x => x)
            .Must(HaveLogicalBuffers)
            .WithMessage(_localizer["Service_Buffers_Logical"]);

        RuleFor(x => x)
            .Must(HaveValidCategorySubcategory)
            .WithMessage(_localizer["Service_CategorySubcategory_Valid"]);

        RuleFor(x => x)
            .Must(BeActiveIfPopular)
            .WithMessage(_localizer["Service_ActiveIfPopular"]);
    }

    // Helper Methods

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
    /// Ensures the total buffer time (before + after) does not exceed the service duration.
    /// </summary>
    private bool HaveLogicalBuffers(ServiceRequestViewModel model)
    {
        var totalBuffer = (model.BufferBeforeMinutes ?? 0) + (model.BufferAfterMinutes ?? 0);
        return totalBuffer <= model.DurationMinutes;
    }

    /// <summary>
    /// Validates that if a subcategory is provided, a main category must also exist.
    /// </summary>
    private bool HaveValidCategorySubcategory(ServiceRequestViewModel model)
    {
        // If there is a subcategory, there must be a main category
        if (!string.IsNullOrEmpty(model.SubCategory))
        {
            return !string.IsNullOrEmpty(model.Category);
        }
        return true;
    }

    /// <summary>
    /// Ensures that popular services are marked as active (inactive popular services are not allowed).
    /// </summary>
    private bool BeActiveIfPopular(ServiceRequestViewModel model)
    {
        // Popular services must be enabled
        if (model.IsPopular)
        {
            return model.IsActive;
        }
        return true;
    }

    [GeneratedRegex(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")]
    private static partial Regex MyRegex();
}

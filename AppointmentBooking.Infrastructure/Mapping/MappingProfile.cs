using AppointmentBooking.Application.ViewModels.Appointment;
using AppointmentBooking.Application.ViewModels.BusinessProfile;
using AppointmentBooking.Application.ViewModels.Customer;
using AppointmentBooking.Application.ViewModels.Service;
using AppointmentBooking.Core.Models;
using Mapster;

namespace AppointmentBooking.Infrastructure.Mapping;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        /*=============Service===============*/
        // Simple mappings (ReverseMap equivalent)
        TypeAdapterConfig<BrandingColors, BrandingColorsViewModel>.NewConfig().TwoWays();
        TypeAdapterConfig<LocalizationConfig, LocalizationConfigViewModel>.NewConfig().TwoWays();
        TypeAdapterConfig<ContactInfo, ContactInfoViewModel>.NewConfig().TwoWays();

        // BusinessProfile mapping (with nested objects)
        TypeAdapterConfig<BusinessProfile, BusinessProfileViewModel>.NewConfig()
            .Map(dest => dest.Colors, src => src.Colors)
            .Map(dest => dest.Localization, src => src.Localization)
            .Map(dest => dest.Contact, src => src.Contact);

        TypeAdapterConfig<BusinessProfileViewModel, BusinessProfile>.NewConfig()
            .Map(dest => dest.Colors, src => src.Colors)
            .Map(dest => dest.Localization, src => src.Localization)
            .Map(dest => dest.Contact, src => src.Contact);

        // Service to ServiceResponseViewModel (with calculated properties)
        TypeAdapterConfig<Service, ServiceResponseViewModel>.NewConfig()
            .Map(dest => dest.AppointmentsCount, src => src.Appointments == null ? 0 : src.Appointments.Count())
            .Map(dest => dest.TotalDuration, src => src.GetTotalDuration());

        // ServiceRequestViewModel to Service (with comma-separated string to List conversion)
        TypeAdapterConfig<ServiceRequestViewModel, Service>.NewConfig()
            .Map(dest => dest.RequiredDocuments, src =>
                !string.IsNullOrWhiteSpace(src.RequiredDocuments)
                ? src.RequiredDocuments.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>());

        // Service to ServiceRequestViewModel (with List to comma-separated string conversion)
        TypeAdapterConfig<Service, ServiceRequestViewModel>.NewConfig()
            .Map(dest => dest.RequiredDocuments, src =>
                src.RequiredDocuments != null && src.RequiredDocuments.Any()
                ? string.Join(",", src.RequiredDocuments)
                : null);


        /*=============Customer===============*/
        // Simple two-way mapping
        TypeAdapterConfig<Customer, CustomerViewModel>.NewConfig().TwoWays();
        TypeAdapterConfig<Appointment, AppointmentViewModel>.NewConfig().TwoWays();

    }
}

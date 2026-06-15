using AppointmentBooking.Application.DTOs;
using AppointmentBooking.Application.ViewModels.Appointment;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentBooking.Application.Interfaces;

public interface IFormPreparationService
{
      /// <summary>
      /// Retrieves cached form data (services and customers lists with their details) using memory cache with double-check locking to prevent multiple concurrent database hits.
      /// </summary>
      Task<FormDataCacheDTO> GetCachedFormDataAsync();

      /// <summary>
      /// Removes the cached form data from memory cache, forcing a fresh load on the next request.
      /// </summary>
      Task InvalidateCacheAsync();

      /// <summary>
      /// Creates a new AppointmentRequestViewModel with default values (next day appointment, Pending status, and optional customer/service IDs).
      /// </summary>
      AppointmentRequestViewModel CreateInitialRequest(int? customerId, int? serviceId, DateTime? date);

      /// <summary>
      /// Populates ViewBag with cached form data (services dropdown, customers dropdown, services data, status list, and edit mode flag) for appointment forms.
      /// </summary>
      Task PopulateViewBagForFormAsync(Controller controller, AppointmentRequestViewModel? model = null, bool isEdit = false);

      /// <summary>
      /// Populates ViewBag with cached services and customers dropdown lists for list views.
      /// </summary>
      Task PopulateViewBagForListAsync(Controller controller);
}

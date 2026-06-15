using AppointmentBooking.Application.DTOs;
using AppointmentBooking.Application.ViewModels.Appointment;

namespace AppointmentBooking.Application.Interfaces;

public interface IAppointmentViewModelService
{
      /// <summary>
      /// Builds an AppointmentListViewModel from filtered appointments with pagination, mapping to ViewModels and applying sorting by date and start time.
      /// </summary>
      Task<AppointmentListViewModel> BuildListViewModelAsync(AppointmentFiltersDTO filters);

      /// <summary>
      /// Builds an AppointmentCalendarViewModel for a specified date range (defaults to current month) with filtered appointments mapped to ViewModels.
      /// </summary>
      Task<AppointmentCalendarViewModel> BuildCalendarViewModelAsync(DateTime? start, string? status);
}

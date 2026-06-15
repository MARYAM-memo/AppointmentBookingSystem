using AppointmentBooking.Application.DTOs;
using AppointmentBooking.Application.Extensions;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.Appointment;
using AppointmentBooking.Core.Models;
using MapsterMapper;

namespace AppointmentBooking.Infrastructure.Services;

public class AppointmentViewModelService(IAppointmentService appointmentService, IMapper mapper) : IAppointmentViewModelService
{
      private readonly IAppointmentService _appointmentService = appointmentService;
      private readonly IMapper _mapper = mapper;

      public async Task<AppointmentListViewModel> BuildListViewModelAsync(AppointmentFiltersDTO filters)
      {
            var appointments = await _appointmentService.GetFilteredAppointmentsAsync(filters);
            var pagedResult = (appointments ?? [])
                .OrderByDescending(b => b.AppointmentDate)
                .ThenBy(b => b.StartTime)
                .ToPagedResult(filters.PageNumber, filters.PageSize);

            return new AppointmentListViewModel
            {
                  Appointments = _mapper.Map<List<AppointmentViewModel>>(pagedResult.Items),
                  SelectedDate = filters.Date,
                  SelectedStatus = filters.Status ?? string.Empty,
                  SelectedServiceId = filters.ServiceId,
                  SelectedCustomerId = filters.CustomerId,
                  SearchTerm = filters.SearchTerm ?? string.Empty,
                  StatusList = BookingStatusExtensions.GetAllStatusesWithLocalizedName(),
                  PageNumber = filters.PageNumber > 0 ? filters.PageNumber : 1,
                  PageSize = filters.PageSize > 0 ? filters.PageSize : 25,
                  TotalCount = pagedResult.Pagination.TotalCount,
                  TotalPages = pagedResult.Pagination.TotalCount.CalculateTotalPages(filters.PageSize)
            };
      }

      public async Task<AppointmentCalendarViewModel> BuildCalendarViewModelAsync(DateTime? start, string? status)
      {
            var startDate = start ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var appointments = await _appointmentService.GetFilteredAppointmentsAsync(
                new AppointmentFiltersDTO { PageSize = int.MaxValue, Status = status }
            );

            var filteredAppointments = appointments?
                .Where(b => b.AppointmentDate >= startDate && b.AppointmentDate <= endDate)
                .ToList() ?? [];

            if (!string.IsNullOrEmpty(status))
            {
                  if (Enum.TryParse<BookingStatus>(status, out var statusEnum))
                  {
                        filteredAppointments = [.. filteredAppointments.Where(a => a.Status == statusEnum)];
                  }
            }
            
            return new AppointmentCalendarViewModel
            {
                  CurrentMonth = startDate,
                  Appointments = _mapper.Map<List<AppointmentViewModel>>(filteredAppointments)
            };
      }
}

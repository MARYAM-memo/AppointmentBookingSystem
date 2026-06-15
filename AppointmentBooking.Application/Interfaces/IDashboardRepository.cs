using AppointmentBooking.Application.DTOs;

namespace AppointmentBooking.Application.Interfaces;

public interface IDashboardRepository
{
      Task<DashboardAggregateDTO> GetDashboardAggregateAsync();
}
using AppointmentBooking.Application.DTOs;
using AppointmentBooking.Application.ViewModels.Dashboard;

namespace AppointmentBooking.Application.Interfaces;

public interface IDashboardService
{
      /// <summary>
      /// Retrieves comprehensive dashboard data including today's stats, growth percentages, weekly revenue chart, top services, and recent appointments.
      /// </summary>
      Task<DashboardViewModel> GetDashboardDataAsync();

      /// <summary>
      /// Retrieves real-time dashboard statistics for today (appointments count, revenue, total customers, cancelled appointments).
      /// </summary>
      Task<DashboardStatsDTO> GetRealtimeStatsAsync();

      /// <summary>
      /// Retrieves today's upcoming appointments sorted by start time, excluding cancelled ones.
      /// </summary>
      Task<List<UpcomingAppointmentDTO>> GetTodayAppointmentsAsync();
}

using AppointmentBooking.Application.DTOs;
namespace AppointmentBooking.Application.ViewModels.Dashboard;

public class DashboardViewModel
{
      // Main Stats
      public int TodayCount { get; set; }
      public decimal TodayRevenue { get; set; }
      public int TotalCustomers { get; set; }
      public int CancelledAppointments { get; set; }

      // Growth
      public decimal AppointmentsGrowth { get; set; }
      public decimal RevenueGrowth { get; set; }
      public int NewCustomersThisMonth { get; set; }
      public decimal CancellationRate { get; set; }

      // Chart Data
      public List<string> WeeklyLabels { get; set; } = [];
      public List<decimal> WeeklyRevenue { get; set; } = [];

      // Lists
      public List<TodayAppointmentDTO> TodayAppointments { get; set; } = [];
      public List<RecentBookingDTO> RecentAppointments { get; set; } = [];
      public List<TopServiceDTO> TopServices { get; set; } = [];
}

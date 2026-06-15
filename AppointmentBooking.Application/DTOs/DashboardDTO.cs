using AppointmentBooking.Application.Extensions;
using AppointmentBooking.Core.Models;

namespace AppointmentBooking.Application.DTOs;

public class DashboardStatsDTO
{
      public int TodayAppointments { get; set; }
      public decimal TodayRevenue { get; set; }
      public int TotalCustomers { get; set; }
      public int CancelledAppointments { get; set; }
}

public class DashboardAggregateDTO
{
      public int TodayCount { get; set; }
      public decimal TodayRevenue { get; set; }
      public int YesterdayCount { get; set; }
      public decimal YesterdayRevenue { get; set; }
      public Dictionary<DateTime, decimal> WeeklyRevenue { get; set; } = [];
      public int TotalCustomers { get; set; }
      public int NewCustomersThisMonth { get; set; }
      public int CancelledAppointments { get; set; }
      public int TotalAppointmentsAllTime { get; set; }
      public int TotalCompletedAppointments { get; set; }
      public List<Appointment> TodayAppointments { get; set; } = [];
      public List<Appointment> RecentAppointments { get; set; } = [];
      public List<TopServiceRawDTO> TopServices { get; set; } = [];
}

public class TopServiceRawDTO
{
      public int ServiceId { get; set; }
      public int Count { get; set; }
      public Service? Service { get; set; }
}

public class TodayAppointmentDTO
{
      public int Id { get; set; }
      public string CustomerName { get; set; } = string.Empty;
      public string ServiceName { get; set; } = string.Empty;
      public TimeSpan StartTime { get; set; }
      public TimeSpan EndTime { get; set; }
      public BookingStatus Status { get; set; }

      public string StatusArabic => Status.GetLocalizedName();

      public string StatusClass => Status.GetCssClass();
}

public class RecentBookingDTO
{
      public int Id { get; set; }
      public string CustomerName { get; set; } = string.Empty;
      public string ServiceName { get; set; } = string.Empty;
      public DateTime AppointmentDate { get; set; }
      public TimeSpan StartTime { get; set; }
      public BookingStatus Status { get; set; }
      public decimal? FinalPrice { get; set; }

      public string StatusArabic => Status.GetLocalizedName();

      public string StatusClass => Status.GetCssClass();
}

public class TopServiceDTO
{
      public string Name { get; set; } = string.Empty;
      public string? Icon { get; set; }
      public int BookingCount { get; set; }
      public double Percentage { get; set; }
}
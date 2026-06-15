using System.Globalization;
using AppointmentBooking.Application.DTOs;
using AppointmentBooking.Application.Extensions;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.Dashboard;
using AppointmentBooking.Core.Models;


namespace AppointmentBooking.Infrastructure.Services;

public class DashboardService(IDashboardRepository dashboardRepository, ILocalizationService localizer) : IDashboardService
{
    readonly IDashboardRepository _dashboardRepository = dashboardRepository;
    readonly ILocalizationService _localizer = localizer;

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var rawData = await _dashboardRepository.GetDashboardAggregateAsync();

        if (rawData == null)
        {
            return new DashboardViewModel();
        }

        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-6);

        return new DashboardViewModel
        {
            // Stats
            TodayCount = rawData.TodayCount,
            TodayRevenue = rawData.TodayRevenue,
            TotalCustomers = rawData.TotalCustomers,
            CancelledAppointments = rawData.CancelledAppointments,

            // Growth percentages
            AppointmentsGrowth = CalculateGrowth(rawData.TodayCount, rawData.YesterdayCount),
            RevenueGrowth = CalculateGrowth(rawData.TodayRevenue, rawData.YesterdayRevenue),
            NewCustomersThisMonth = rawData.NewCustomersThisMonth,
            CancellationRate = CalculateCancellationRate(rawData.CancelledAppointments, rawData.TotalAppointmentsAllTime),

            // Chart data
            WeeklyLabels = GetWeeklyLabels(today, _localizer),
            WeeklyRevenue = GetWeeklyRevenueList(rawData.WeeklyRevenue, weekAgo, today),

            // Lists
            TodayAppointments = MapTodayAppointments(rawData.TodayAppointments, _localizer),
            RecentAppointments = MapRecentAppointments(rawData.RecentAppointments, _localizer),
            TopServices = MapTopServices(rawData.TopServices, rawData.TotalCompletedAppointments, _localizer)
        };
    }

    public async Task<DashboardStatsDTO> GetRealtimeStatsAsync()
    {
        var rawData = await _dashboardRepository.GetDashboardAggregateAsync();

        return new DashboardStatsDTO
        {
            TodayAppointments = rawData.TodayCount,
            TodayRevenue = rawData.TodayRevenue,
            TotalCustomers = rawData.TotalCustomers,
            CancelledAppointments = rawData.CancelledAppointments
        };
    }

    public async Task<List<UpcomingAppointmentDTO>> GetTodayAppointmentsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var now = DateTime.UtcNow.TimeOfDay;

        var rawData = await _dashboardRepository.GetDashboardAggregateAsync();

        return [.. rawData.TodayAppointments
            .Where(a => a.StartTime >= now && a.Status != BookingStatus.Cancelled)
            .OrderBy(a => a.StartTime)
            .Select(a => new UpcomingAppointmentDTO
            {
                Id = a.Id,
                CustomerName = a.Customer?.FullName ?? _localizer["Common_Unknown"],
                ServiceName = a.Service?.Name ?? _localizer["Common_Unknown"],
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status.GetLocalizedName(),
                PhoneNumber = a.Customer?.PhoneNumber
            })];
    }

    // Helper methods

    /// <summary>
    /// Calculates growth percentage between current and previous values (handles zero division).
    /// </summary>
    private static decimal CalculateGrowth(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return (current - previous) / previous * 100;
    }

    /// <summary>
    /// Calculates cancellation rate as percentage of cancelled appointments to total appointments.
    /// </summary>
    private static decimal CalculateCancellationRate(int cancelled, int total)
    {
        return total > 0 ? Math.Round((decimal)cancelled / total * 100, 1) : 0;
    }

    /// <summary>
    /// Generates Arabic labels for the last 7 days (today, yesterday, or day names in Arabic).
    /// </summary>
    private static List<string> GetWeeklyLabels(DateTime today, ILocalizationService localizer)
    {
        var labels = new List<string>();
        for (var i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var label = i == 0 ? localizer["Common_Today"] : i == 1 ? localizer["Common_Yesterday"] : date.ToString("ddd", CultureInfo.CurrentCulture);
            labels.Add(label);
        }
        return labels;
    }

    /// <summary>
    /// Converts daily revenue dictionary into a list for the last 7 days in chronological order.
    /// </summary>
    private static List<decimal> GetWeeklyRevenueList(Dictionary<DateTime, decimal> revenue, DateTime start, DateTime end)
    {
        var result = new List<decimal>();
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            result.Add(revenue.GetValueOrDefault(date.Date, 0));
        }
        return result;
    }

    /// <summary>
    /// Maps appointment entities to TodayAppointmentDTO with customer and service details.
    /// </summary>
    private static List<TodayAppointmentDTO> MapTodayAppointments(List<Appointment> appointments, ILocalizationService localizer)
    {
        return [.. appointments
            .Where(a => a.Status != BookingStatus.Cancelled)
            .Select(a => new TodayAppointmentDTO
            {
                Id = a.Id,
                CustomerName = a.Customer?.FullName ?? localizer["Common_Unknown"],
                ServiceName = a.Service?.Name ?? localizer["Common_Unknown"],
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status
            })
            .OrderBy(a => a.StartTime)];
    }

    /// <summary>
    /// Maps appointment entities to RecentBookingDTO with customer, service, and pricing details.
    /// </summary>
    private static List<RecentBookingDTO> MapRecentAppointments(List<Appointment> appointments, ILocalizationService localizer)
    {
        return [.. appointments.Select(b => new RecentBookingDTO
        {
            Id = b.Id,
            CustomerName = b.Customer?.FullName ?? localizer["Common_Unknown"],
            ServiceName = b.Service?.Name ?? localizer["Common_Unknown"],
            AppointmentDate = b.AppointmentDate,
            StartTime = b.StartTime,
            Status = b.Status,
            FinalPrice = b.FinalPrice ?? b.TotalPrice
        })];
    }

    /// <summary>
    /// Maps raw top services data to TopServiceDTO list, calculating percentage based on total completed appointments and using localized fallback for unknown service names.
    /// </summary>
    private static List<TopServiceDTO> MapTopServices(List<TopServiceRawDTO> topServicesRaw, int totalCompleted, ILocalizationService localizer)
    {
        return [.. topServicesRaw.Select(s => new TopServiceDTO
        {
            Name = s.Service?.Name ?? localizer["Common_Unknown"],
            Icon = s.Service?.Icon,
            BookingCount = s.Count,
            Percentage = totalCompleted > 0 ? (double)s.Count / totalCompleted * 100 : 0
        })];
    }
}

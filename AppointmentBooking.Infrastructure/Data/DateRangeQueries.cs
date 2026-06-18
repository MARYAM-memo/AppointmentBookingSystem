using AppointmentBooking.Core.Interfaces;
using AppointmentBooking.Core.Models;

namespace AppointmentBooking.Infrastructure.Data;

public static class DateRangeQueries
{
    /// <summary>
    /// Extension method that retrieves appointment statistics (total count, total revenue, and cancelled count) within a date range.
    /// </summary>
    public static async Task<(int TotalCount, decimal TotalRevenue, int CancelledCount)> GetAppointmentsStatsAsync(this IUnitOfWork unitOfWork, DateTime startDate, DateTime endDate, bool excludeCancelled = true)
    {
        var appointments = await unitOfWork.Appointments.FetchAsync(
            b => b.AppointmentDate.Date >= startDate.Date &&
                 b.AppointmentDate.Date <= endDate.Date &&
                 !b.IsDeleted &&
                 (!excludeCancelled || (b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.NoShow)),
            asNoTracking: true
        );

        return (
            TotalCount: appointments.Count(),
            TotalRevenue: appointments.Where(b => b.FinalPrice.HasValue).Sum(b => b.FinalPrice ?? 0),
            CancelledCount: appointments.Count(b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.NoShow)
        );
    }

    /// <summary>
    /// Extension method that calculates daily revenue grouped by date within the specified date range.
    /// </summary>
    public static async Task<Dictionary<DateTime, decimal>> GetDailyRevenueAsync(this IUnitOfWork unitOfWork, DateTime startDate, DateTime endDate)
    {
        var completedAppointments = await unitOfWork.Appointments.FetchAsync(
            b => b.AppointmentDate.Date >= startDate.Date &&
                 b.AppointmentDate.Date <= endDate.Date &&
                 !b.IsDeleted &&
                 b.Status != BookingStatus.Cancelled &&
                 b.Status != BookingStatus.NoShow &&
                 b.FinalPrice.HasValue,
            asNoTracking: true
        );

        return completedAppointments
            .GroupBy(b => b.AppointmentDate.Date)
            .ToDictionary(g => g.Key, g => g.Sum(b => b.FinalPrice ?? 0));
    }

}

using AppointmentBooking.Application.DTOs;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentBooking.Infrastructure.DataAccess;


public class DashboardRepository(DatabaseContext context) : IDashboardRepository
{
  private readonly DatabaseContext _context = context;

  public async Task<DashboardAggregateDTO> GetDashboardAggregateAsync()
  {
    var today = DateTime.UtcNow.Date;
    var yesterday = today.AddDays(-1);
    var weekAgo = today.AddDays(-6);
    var monthStart = new DateTime(today.Year, today.Month, 1);

    var weeklyRevenueData = await _context.Appointments
        .Where(b => b.AppointmentDate >= weekAgo && b.AppointmentDate < today.AddDays(1) && !b.IsDeleted)
        .GroupBy(b => b.AppointmentDate.Date)
        .Select(g => new { Date = g.Key, Revenue = g.Sum(b => b.FinalPrice ?? b.TotalPrice) ?? 0m })
        .ToListAsync();

    var weeklyRevenueDict = weeklyRevenueData.ToDictionary(x => x.Date, x => x.Revenue);

    // Query واحد يجيب كل حاجة
    var result = await (from b in _context.Appointments
                        join c in _context.Customers on b.CustomerId equals c.Id into customers
                        from customer in customers.DefaultIfEmpty()
                        join s in _context.Services on b.ServiceId equals s.Id into services
                        from service in services.DefaultIfEmpty()
                        where !b.IsDeleted
                        group b by 1 into g
                        select new DashboardAggregateDTO
                        {
                          // Today's stats
                          TodayCount = _context.Appointments.Count(b => b.AppointmentDate.Date == today && !b.IsDeleted),
                          TodayRevenue = _context.Appointments.Where(b => b.AppointmentDate.Date == today && !b.IsDeleted).Sum(b => b.FinalPrice ?? b.TotalPrice) ?? 0m,

                          // Yesterday's stats
                          YesterdayCount = _context.Appointments.Count(b => b.AppointmentDate.Date == yesterday && !b.IsDeleted),
                          YesterdayRevenue = _context.Appointments.Where(b => b.AppointmentDate.Date == yesterday && !b.IsDeleted).Sum(b => b.FinalPrice ?? b.TotalPrice) ?? 0m,

                          // Customer counts
                          TotalCustomers = _context.Customers.Count(c => !c.IsDeleted),
                          NewCustomersThisMonth = _context.Customers.Count(c => c.CreatedAt >= monthStart && !c.IsDeleted),

                          // Booking counts
                          CancelledAppointments = _context.Appointments.Count(b => (b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.NoShow )&& !b.IsDeleted),
                          TotalAppointmentsAllTime = _context.Appointments.Count(b => !b.IsDeleted),
                          TotalCompletedAppointments = _context.Appointments.Count(b => b.Status == BookingStatus.Completed && !b.IsDeleted),

                          // Lists (Take 10)
                          TodayAppointments = _context.Appointments
                                .Where(b => b.AppointmentDate.Date == today && !b.IsDeleted)
                                .Include(b => b.Service)
                                .Include(b => b.Customer)
                                .Take(50)
                                .ToList(),

                          RecentAppointments = _context.Appointments
                                .Where(b => !b.IsDeleted)
                                .Include(b => b.Service)
                                .Include(b => b.Customer)
                                .OrderByDescending(b => b.AppointmentDate)
                                .ThenByDescending(b => b.CreatedAt)
                                .Take(10)
                                .ToList(),

                          TopServices = _context.Appointments
                                .Where(b => b.Status == BookingStatus.Completed && !b.IsDeleted)
                                .GroupBy(b => b.ServiceId)
                                .Select(g => new TopServiceRawDTO
                                {
                                  ServiceId = g.Key,
                                  Count = g.Count(),
                                  Service = _context.Services.FirstOrDefault(s => s.Id == g.Key)
                                })
                                .OrderByDescending(x => x.Count)
                                .Take(5)
                                .ToList()
                        }).FirstOrDefaultAsync();

    if (result != null)
    {
      result.WeeklyRevenue = weeklyRevenueDict;
    }
    return result ?? new DashboardAggregateDTO();
  }
}

using AppointmentBooking.Application.Exceptions;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentBooking.Web.Controllers
{
    [Authorize]
    public class DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger) : BaseController
    {
        readonly IDashboardService _dashboardService = dashboardService;
        readonly ILogger<DashboardController> _logger = logger;
        public async Task<IActionResult> Index()
        {
            await SetCustomLabelsAsync();
            DashboardViewModel dashboardData = new() { };
            try { dashboardData = await _dashboardService.GetDashboardDataAsync(); }
            catch (DomainException exception)
            {
                _logger.LogError(exception, "Dashboard load failed");
            }

            return View(dashboardData);
        }

        public async Task<IActionResult> Stats()
        {
            var stats = await _dashboardService.GetRealtimeStatsAsync();
            return Json(stats);
        }

        public async Task<IActionResult> TodayAppointments()
        {
            var appointments = await _dashboardService.GetTodayAppointmentsAsync();
            return Json(appointments);
        }

    }
}

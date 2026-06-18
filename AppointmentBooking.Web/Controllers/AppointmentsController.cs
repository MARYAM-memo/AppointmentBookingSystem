using AppointmentBooking.Application.DTOs;
using AppointmentBooking.Application.Exceptions;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.Appointment;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Infrastructure.Services;
using AppointmentBooking.Application.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using AppointmentBooking.Application.Shared;

namespace AppointmentBooking.Web.Controllers;

[Authorize]
[EnableRateLimiting("strict")]
public class AppointmentsController(IAvailabilityService availabilityService, IValidator<AppointmentRequestViewModel> validator, IAppointmentService appointmentService, IFormPreparationService formPreparation, IAppointmentViewModelService viewModelService, ILogger<AppointmentsController> logger) : BaseController
{
    private readonly IAvailabilityService _availabilityService = availabilityService;
    readonly IValidator<AppointmentRequestViewModel> _validator = validator;
    readonly IAppointmentService _appointmentService = appointmentService;
    private readonly IFormPreparationService _formPreparation = formPreparation;
    private readonly IAppointmentViewModelService _viewModelService = viewModelService;
    readonly ILogger<AppointmentsController> _logger = logger;

    #region Core Actions

    public async Task<IActionResult> Index(AppointmentFiltersDTO filters)
    {
        await SetCustomLabelsAsync();

        // Get current currency from profile
        var profile = await GetCurrentProfileAsync();
        var currency = profile?.Localization?.Currency ?? PriceExtension.CurrentCurrency;

        await _formPreparation.PopulateViewBagForListAsync(this, currency: currency);

        var viewModel = await _viewModelService.BuildListViewModelAsync(filters);
        return View(viewModel);
    }

    [Authorize(Roles = Constants.AdminRole)]
    public async Task<IActionResult> Create(int? customerId, int? serviceId, DateTime? date)
    {
        await SetViewBagDataAsync();

        // Get current currency from profile
        var profile = await GetCurrentProfileAsync();
        var currency = profile?.Localization?.Currency ?? PriceExtension.CurrentCurrency;

        await _formPreparation.PopulateViewBagForFormAsync(this, isEdit: false, currency: currency);

        var model = _formPreparation.CreateInitialRequest(customerId, serviceId, date);
        return View("Form", model);
    }

    [HttpPost]
    [Authorize(Roles = Constants.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentRequestViewModel model)
    {
        return await ValidateAndExecuteAsync<AppointmentRequestViewModel, IValidator<AppointmentRequestViewModel>>(
            model, "Form",
            onFailure: async () => await HandleInvalidModelAsync(model, isEdit: false),
            onSuccess: async () =>
            {
                try
                {
                    var appointment = await _appointmentService.CreateAppointmentAsync(model, User.Identity?.Name);
                    SuccessMessage(Localizer["Appointment_CreateSuccess"]);
                    await _formPreparation.InvalidateCacheAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DomainException ex) when (ex.InnerException is DbUpdateException dbEx && AppointmentService.IsUniqueConstraintViolation(dbEx))
                {
                    ErrorMessage(ex.Message);
                    ViewBag.ConflictDetected = true;
                    ViewBag.ConflictServiceId = model.ServiceId;
                    ViewBag.ConflictDate = model.AppointmentDate.ToString("yyyy-MM-dd");
                    return await HandleInvalidModelAsync(model, isEdit: false);
                }
            }
        );
    }
    
    [Authorize(Roles = Constants.AdminRole)]
    public async Task<IActionResult> Edit(int id)
    {
        var appointment = await UnitOfWork.Appointments.FindFirstOrDefaultAsync(
           predicate: b => b.Id == id && !b.IsDeleted,
           b => b.Service,
           b => b.Customer);

        if (appointment == null) return NotFoundAppointment();

        await SetViewBagDataAsync();

        // Get current currency from profile
        var profile = await GetCurrentProfileAsync();
        var currency = profile?.Localization?.Currency ?? PriceExtension.CurrentCurrency;

        await _formPreparation.PopulateViewBagForFormAsync(this, isEdit: true, currency: currency);

        var model = Mapper.Map<AppointmentRequestViewModel>(appointment);
        return View("Form", model);
    }

    [HttpPost]
    [Authorize(Roles = Constants.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppointmentRequestViewModel model)
    {
        if (id != model.Id) return BadRequest();

        return await ValidateAndExecuteAsync<AppointmentRequestViewModel, IValidator<AppointmentRequestViewModel>>(
            model, "Form",
            onFailure: async () => await HandleInvalidModelAsync(model, isEdit: true),
            onSuccess: async () =>
            {
                try
                {
                    await _appointmentService.UpdateAppointmentAsync(id, model);
                    SuccessMessage(Localizer["Appointment_UpdateSuccess"]);
                    await _formPreparation.InvalidateCacheAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DomainException ex) when (ex.InnerException is DbUpdateConcurrencyException)
                {
                    ErrorMessage(ex.Message);
                    ViewBag.ConflictDetected = true;  // <-- Auto-refresh flag
                    ViewBag.ConflictServiceId = model.ServiceId;
                    ViewBag.ConflictDate = model.AppointmentDate.ToString("yyyy-MM-dd");
                    return await HandleInvalidModelAsync(model, isEdit: true);
                }
            }
        );

    }

    public async Task<IActionResult> Details(int id)
    {
        var appointment = await UnitOfWork.Appointments.FindFirstOrDefaultAsync(
           predicate: b => b.Id == id && !b.IsDeleted,
           orderBy: b => b.AppointmentDate,
           b => b.Service,
           b => b.Customer);

        if (appointment == null) return NotFoundAppointment();

        await SetCustomLabelsAsync();
        return View(Mapper.Map<AppointmentViewModel>(appointment));
    }

    public async Task<IActionResult> Calendar(DateTime? startDate, string? status = null)
    {
        await SetCustomLabelsAsync();
        if (!string.IsNullOrEmpty(status)) ViewBag.CurrentStatusFilter = status;
        var viewModel = await _viewModelService.BuildCalendarViewModelAsync(startDate, status);
        return View(viewModel);
    }

    #endregion

    #region Helper Actions (AJAX)

    public async Task<IActionResult> GetAvailableSlots(int serviceId, DateTime date, int? excludeAppointmentId = null)
    {
        var slots = await _appointmentService.IsSlotAvailableAsync(serviceId, date, TimeSpan.Zero, excludeAppointmentId)
            ? await _availabilityService.GetAvailableSlotsAsync(serviceId, date, excludeAppointmentId)
            : [];

        return Json(slots);
    }

    [HttpPost]
    [Authorize(Roles = Constants.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, BookingStatus status)
    {
        try
        {
            await _appointmentService.UpdateStatusAsync(id, status);
            SuccessMessage(string.Format(Localizer["Appointment_StatusUpdateSuccess"], status.GetActionText()));
        }
        catch (DomainException ex)
        {
            ErrorMessage(ex.Message);
            _logger.LogError(ex, "Appointment Updated Failed");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = Constants.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _appointmentService.DeleteAppointmentAsync(id);
            SuccessMessage(Localizer["Appointment_DeleteSuccess"]);
            await _formPreparation.InvalidateCacheAsync();
        }
        catch (DomainException ex)
        {
            ErrorMessage(ex.Message);
            _logger.LogError(ex, "Appointment Deleted Failed");
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Handles invalid model state by displaying error messages and repopulating form data.
    /// </summary>
    private async Task<IActionResult> HandleInvalidModelAsync(AppointmentRequestViewModel model, bool isEdit)
    {
        var profile = await GetCurrentProfileAsync();
        var currency = profile?.Localization?.Currency ?? PriceExtension.CurrentCurrency;
        await _formPreparation.PopulateViewBagForFormAsync(this, model, isEdit, currency);
        await SetCustomLabelsAsync();
        return View("Form", model);
    }

    /// <summary>
    /// Returns a NotFoundResult and displays an error message that the appointment was not found.
    /// </summary>
    private NotFoundResult NotFoundAppointment()
    {
        ErrorMessage(Localizer["Appointment_NotFound"]);
        return NotFound();
    }
    #endregion
}

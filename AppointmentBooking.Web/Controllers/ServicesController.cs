using AppointmentBooking.Application.ViewModels.Service;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Application.Shared;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentBooking.Web.Controllers
{
    [Authorize]
    public class ServicesController(IValidator<ServiceRequestViewModel> validator) : BaseController
    {
        readonly IValidator<ServiceRequestViewModel> _validator = validator;
        public async Task<ActionResult> Index(string? searchTerm, string? category, bool? includeInactive)
        {
            await SetViewBagDataAsync();

            var services = await SearchServicesAsync(searchTerm);

            // Get categories for filter dropdown
            var categories = services
                .Where(s => !string.IsNullOrEmpty(s.Category))
                .Select(s => s.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            if (!string.IsNullOrWhiteSpace(category))
            {
                services = services.Where(s => s.Category == category);
            }

            if (includeInactive != true)
            {
                services = services.Where(s => s.IsActive);
            }

            var serviceList = services.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name).ToList();
            for (int i = 0; i < serviceList.Count; i++)
            {
                var srv = serviceList[i];
                Console.WriteLine($"{i}: {srv?.Appointments?.Count}");
            }
            var model = new ServiceListViewModel
            {
                Services = Mapper.Map<IEnumerable<ServiceResponseViewModel>>(serviceList),
                Categories = categories,
                SearchTerm = searchTerm,
                SelectedCategory = category,
                ShowInactive = includeInactive == true,
            };

            return View(model);
        }

        public async Task<ActionResult> Details(int id)
        {
            var service = await UnitOfWork.Services.FindFirstOrDefaultAsync(predicate:
            s => s.Id == id, orderBy: x => x.Id, args: s => s.Appointments!.Where(b => b.ServiceId == id && !b.IsDeleted)) ?? throw new KeyNotFoundException(string.Format(Localizer["Service_NotFound"], id));

            await SetCustomLabelsAsync();
            var appointments = service.Appointments;
            ViewBag.BookingCount = appointments?.Count ?? 0;

            var model = Mapper.Map<ServiceResponseViewModel>(service);
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await SetCustomLabelsAsync();
            ViewBag.IsEdit = false;

            return View("Form", new ServiceRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestViewModel model)
        {
            return await ValidateAndExecuteAsync<ServiceRequestViewModel, IValidator<ServiceRequestViewModel>>(
                model, "Form",
                onFailure: async () =>
                {
                    await SetCustomLabelsAsync();
                    ViewBag.IsEdit = false;
                    return View("Form", model);
                },
                onSuccess: async () =>
                {
                    var service = Mapper.Map<Service>(model);
                    service.Duration = TimeSpan.FromMinutes(model.DurationMinutes);
                    service.BufferBefore = model.BufferBeforeMinutes.HasValue
                        ? TimeSpan.FromMinutes(model.BufferBeforeMinutes.Value)
                        : null;
                    service.BufferAfter = model.BufferAfterMinutes.HasValue
                        ? TimeSpan.FromMinutes(model.BufferAfterMinutes.Value)
                        : null;
                    service.RequiredDocuments = ParseRequiredDocuments(model.RequiredDocuments);

                    UnitOfWork.Services.Add(service);
                    await UnitOfWork.SaveChangesAsync();

                    SuccessMessage(string.Format(Localizer["Service_CreateSuccess"], model.Name));
                    return RedirectToAction(nameof(Index));
                }
            );
        }

        public async Task<IActionResult> Edit(int id)
        {
            var service = await UnitOfWork.Services.FindByIdAsync(id);
            if (service == null || service.IsDeleted)
                return NotFoundService(id);

            await SetCustomLabelsAsync();
            ViewBag.IsEdit = true;

            var model = Mapper.Map<ServiceRequestViewModel>(service);
            model.DurationMinutes = (int)service.Duration.TotalMinutes;
            model.BufferBeforeMinutes = service.BufferBefore != null ? (int)service.BufferBefore.Value.TotalMinutes : null;
            model.BufferAfterMinutes = service.BufferAfter != null ? (int)service.BufferAfter.Value.TotalMinutes : null;
            model.RequiredDocuments = service.RequiredDocuments != null
                ? string.Join(", ", service.RequiredDocuments)
                : null;
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequestViewModel model)
        {
            if (id != model.Id)
                return BadRequest(Localizer["Service_IdMismatch"]);

            return await ValidateAndExecuteAsync<ServiceRequestViewModel, IValidator<ServiceRequestViewModel>>(
                model, "Form",
                onFailure: async () =>
                {
                    await SetCustomLabelsAsync();
                    ViewBag.IsEdit = true;
                    return View("Form", model);
                },
                onSuccess: async () =>
                {
                    var service = await UnitOfWork.Services.FindByIdAsync(id);
                    if (service == null || service.IsDeleted)
                        return NotFoundService(id); Mapper.Map(model, service);
                    service.BufferBefore = model.BufferBeforeMinutes.HasValue
                    ? TimeSpan.FromMinutes(model.BufferBeforeMinutes.Value)
                    : null;
                    service.BufferAfter = model.BufferAfterMinutes.HasValue
                        ? TimeSpan.FromMinutes(model.BufferAfterMinutes.Value)
                        : null;
                    service.Duration = TimeSpan.FromMinutes(model.DurationMinutes);
                    service.RequiredDocuments = ParseRequiredDocuments(model.RequiredDocuments);
                    UnitOfWork.Services.Update(service);
                    await UnitOfWork.SaveChangesAsync();
                    SuccessMessage(string.Format(Localizer["Service_UpdateSuccess"], model.Name));
                    return RedirectToAction(nameof(Index));
                }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var service = await UnitOfWork.Services.FindByIdAsync(id);
            if (service == null || service.IsDeleted) return NotFoundService(id);

            service.IsActive = !service.IsActive;
            service.UpdatedAt = DateTime.UtcNow;

            await UnitOfWork.SaveChangesAsync();

            var action = service.IsActive ? Localizer["Service_ToggleStatus_Activate"] : Localizer["Service_ToggleStatus_Deactivate"];
            SuccessMessage(string.Format(Localizer["Service_ToggleStatus_Success"], action));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePopular(int id)
        {
            var service = await UnitOfWork.Services.FindByIdAsync(id);
            if (service == null || service.IsDeleted) return NotFoundService(id);

            service.IsPopular = !service.IsPopular;
            service.UpdatedAt = DateTime.UtcNow;

            await UnitOfWork.SaveChangesAsync();

            SuccessMessage(Localizer["Service_TogglePopular_Success"]);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await UnitOfWork.Services.FindByIdAsync(id);
            if (service == null || service.IsDeleted) return NotFoundService(id);

            // Soft delete
            service.IsDeleted = true;
            service.DeletedAt = DateTime.UtcNow;
            service.IsActive = false;

            await UnitOfWork.SaveChangesAsync();

            SuccessMessage(Localizer["Service_DeleteSuccess"]);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Returns a NotFoundResult and displays an error message that the service was not found.
        /// </summary>
        NotFoundResult NotFoundService(int id)
        {
            ErrorMessage(string.Format(Localizer["Service_NotFound"], id));
            return NotFound();
        }

        /// <summary>
        /// Splits a comma or newline-separated string into a list of trimmed, non-empty document names. Returns null if input is null or empty.
        /// </summary>
        private static List<string>? ParseRequiredDocuments(string? documentsInput)
        {
            return documentsInput?
                .Split([',', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
        /// <summary>
        /// Searches for active services by name, description, or category using a SQL LIKE pattern with escaped special characters for safe wildcard searching.
        /// </summary>
        private async Task<IEnumerable<Service>> SearchServicesAsync(string? searchTerm)
        {
            var query = UnitOfWork.Services.GetQueryable(s => s.Appointments!);
            query = query.Where(s => !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchPattern = $"%{Methods.EscapeForLike(searchTerm.Trim())}%";

                query = query.Where(s =>
                    EF.Functions.Like(s.Name, searchPattern, "\\") ||
                    (s.Description != null && EF.Functions.Like(s.Description, searchPattern, "\\")) ||
                    (s.Category != null && EF.Functions.Like(s.Category, searchPattern, "\\"))
                );
            }

            return await query.AsNoTracking().ToListAsync();
        }
    }
}

using AppointmentBooking.Application.DTOs;
using AppointmentBooking.Application.Extensions;
using AppointmentBooking.Application.ViewModels.Appointment;
using AppointmentBooking.Application.ViewModels.Customer;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Application.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AppointmentBooking.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("default")]
    public class CustomersController(IValidator<CustomerRequestViewModel> validator) : BaseController
    {
        readonly IValidator<CustomerRequestViewModel> _validator = validator;

        public async Task<IActionResult> Index(FilterDTO filter)
        {
            await SetCustomLabelsAsync();

            var query = UnitOfWork.Customers.GetQueryable(s => s.Appointments!);
            query = query.Where(s => !s.IsDeleted);

            // Search
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim();
                var searchPattern = $"%{Methods.EscapeForLike(searchTerm)}%";

                query = query.Where(c =>
                    EF.Functions.Like(c.FullName, searchPattern, "\\") ||
                    EF.Functions.Like(c.PhoneNumber, searchPattern, "\\") ||
                    (c.Email != null && EF.Functions.Like(c.Email, searchPattern, "\\"))
                );
            }
            // Sorting
            query = filter.SortBy switch
            {
                "name" => query.OrderBy(x => x.FullName),
                "appointments" => query.OrderByDescending(c => c.TotalAppointments),
                "recent" => query.OrderByDescending(c => c.LastAppointmentDate ?? c.CreatedAt),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            var customers = await query.AsNoTracking().ToListAsync();

            var paged = customers.ToPagedResult(filter.PageNumber, filter.PageSize);
            var totalCount = paged.Pagination.TotalCount;
            var viewModel = new CustomerListViewModel
            {
                Customers = Mapper.Map<List<CustomerViewModel>>(paged.Items.ToList()),
                SearchTerm = filter.SearchTerm,
                SortBy = filter.SortBy,
                PageNumber = filter.PageNumber > 0 ? filter.PageNumber : 1,
                PageSize = filter.PageSize > 0 ? filter.PageSize : 25,
                TotalCount = totalCount,
                TotalPages = totalCount.CalculateTotalPages(filter.PageSize)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            await SetCustomLabelsAsync();
            ViewBag.IsEdit = false;

            return View("Form", new CustomerRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerRequestViewModel model)
        {
            return await ValidateAndExecuteAsync<CustomerRequestViewModel, IValidator<CustomerRequestViewModel>>(
                model, "Form",
                onFailure: async () =>
                {
                    await SetCustomLabelsAsync();
                    ViewBag.IsEdit = false;
                    return View("Form", model);
                },
                onSuccess: async () =>
                {
                    var customer = new Customer
                    {
                        FullName = model.FullName,
                        PhoneNumber = model.PhoneNumber,
                        Email = model.Email,
                        Notes = model.Notes,
                        TotalAppointments = 0,
                        CreatedAt = DateTime.UtcNow
                    };

                    UnitOfWork.Customers.Add(customer);
                    await UnitOfWork.SaveChangesAsync();

                    SuccessMessage(string.Format(Localizer["Customer_CreateSuccess"], model.FullName));
                    return RedirectToAction(nameof(Index));
                }
            );
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await UnitOfWork.Customers.FindByIdAsync(id);
            if (customer == null || customer.IsDeleted)
                return NotFound();

            await SetCustomLabelsAsync();
            ViewBag.IsEdit = true;

            var model = new CustomerRequestViewModel
            {
                Id = customer.Id,
                FullName = customer.FullName,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                Notes = customer.Notes
            };

            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerRequestViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            return await ValidateAndExecuteAsync<CustomerRequestViewModel, IValidator<CustomerRequestViewModel>>(
              model, "Form",
              onFailure: async () =>
              {
                  await SetCustomLabelsAsync();
                  ViewBag.IsEdit = true;
                  return View("Form", model);
              },
              onSuccess: async () =>
              {
                  var customer = await UnitOfWork.Customers.FindByIdAsync(id);
                  if (customer == null || customer.IsDeleted)
                      return NotFound();

                  customer.FullName = model.FullName;
                  customer.PhoneNumber = model.PhoneNumber;
                  customer.Email = model.Email;
                  customer.Notes = model.Notes;
                  customer.UpdatedAt = DateTime.UtcNow;

                  UnitOfWork.Customers.Update(customer);
                  await UnitOfWork.SaveChangesAsync();

                  SuccessMessage(string.Format(Localizer["Customer_UpdateSuccess"], model.FullName));
                  return RedirectToAction(nameof(Index));
              }
          );
        }

        // GET: /Customers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var customer = await UnitOfWork.Customers.FindFirstOrDefaultAsync(
                c => c.Id == id,
                orderBy: x => x.Id,
                args: s => s.Appointments!.Where(b => b.CustomerId == id && !b.IsDeleted)
            );

            if (customer == null)
                return NotFound();

            await SetCustomLabelsAsync();
            // Get appointments with services
            var appointments = await UnitOfWork.Appointments.FetchAsync(
                a => a.CustomerId == id && !a.IsDeleted,
                asNoTracking: true,
                a => a.Service!
            );
            appointments = [.. appointments.OrderByDescending(a => a.AppointmentDate)];
            var viewModel = new CustomerResponseViewModel
            {
                Customer = Mapper.Map<CustomerViewModel>(customer),
                Appointments = Mapper.Map<List<AppointmentViewModel>>(appointments),
                TotalSpent = appointments.Where(a => a.FinalPrice.HasValue).Sum(a => a.FinalPrice ?? 0)
            };

            return View(viewModel);
        }

        // POST: /Customers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await UnitOfWork.Customers.FindByIdAsync(id);
            if (customer == null || customer.IsDeleted)
                return NotFound();

            // Soft delete
            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;

            await UnitOfWork.SaveChangesAsync();

            SuccessMessage(Localizer["Customer_DeleteSuccess"]);
            return RedirectToAction(nameof(Index));
        }

    }
}

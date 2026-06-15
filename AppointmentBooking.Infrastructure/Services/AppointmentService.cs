using AppointmentBooking.Application.DTOs;
using AppointmentBooking.Application.Exceptions;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.Appointment;
using AppointmentBooking.Core.Interfaces;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Application.Shared;
using Ganss.Xss;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AppointmentBooking.Infrastructure.Services;

public class AppointmentService(IUnitOfWork unitOfWork, IAvailabilityService availabilityService, ILogger<AppointmentService> logger, IBusinessProfileService businessProfile, ILocalizationService localizationService) : IAppointmentService
{
      readonly IUnitOfWork _unitOfWork = unitOfWork;
      readonly IAvailabilityService _availabilityService = availabilityService;
      readonly ILogger<AppointmentService> _logger = logger;
      readonly IBusinessProfileService _businessProfile = businessProfile;
      readonly ILocalizationService _localizationService = localizationService;

      public async Task<IEnumerable<Appointment>?> GetFilteredAppointmentsAsync(AppointmentFiltersDTO filters)
      {
            // FIXED: No more AsEnumerable() - filter at database level
            var query = _unitOfWork.Appointments.GetQueryable(
                b => b.Service!,
                b => b.Customer!
            );

            query = query.Where(b => !b.IsDeleted);

            // Apply filters BEFORE materializing
            if (filters.Date.HasValue)
                  query = query.Where(b => b.AppointmentDate.Date == filters.Date.Value.Date);

            if (!string.IsNullOrEmpty(filters.Status) && Enum.TryParse<BookingStatus>(filters.Status, out var status))
                  query = query.Where(b => b.Status == status);

            if (filters.ServiceId.HasValue)
                  query = query.Where(b => b.ServiceId == filters.ServiceId.Value);

            if (filters.CustomerId.HasValue)
                  query = query.Where(b => b.CustomerId == filters.CustomerId.Value);

            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                  var searchTerm = filters.SearchTerm.Trim();
                  var searchPattern = $"%{Methods.EscapeForLike(searchTerm)}%";

                  query = query.Where(b =>
                      EF.Functions.Like(b.Customer!.FullName, searchPattern, "\\") ||
                      EF.Functions.Like(b.Service!.Name, searchPattern, "\\") ||
                      (b.Notes != null && EF.Functions.Like(b.Notes, searchPattern, "\\"))
                  );
            }

            return await query.OrderByDescending(b => b.AppointmentDate).ThenBy(b => b.StartTime).AsNoTracking().ToListAsync();
      }

      public async Task<Appointment> CreateAppointmentAsync(AppointmentRequestViewModel model, string? createdByUserId)
      {
            // Check availability
            if (!await _availabilityService.IsSlotAvailableAsync(model.ServiceId, model.AppointmentDate, model.StartTime))
                  throw new DomainException(_localizationService["Appointment_Time_NotAvailable"]);

            var service = await GetServiceOrThrowAsync(model.ServiceId);

            var appointment = new Appointment
            {
                  CustomerId = model.CustomerId,
                  ServiceId = model.ServiceId,
                  AppointmentDate = model.AppointmentDate,
                  StartTime = model.StartTime,
                  Status = model.Status,
                  TotalPrice = service.Price,
                  DiscountAmount = model.DiscountAmount,
                  FinalPrice = CalculateFinalPrice(service.Price, model.DiscountAmount),
                  Currency = (await _businessProfile.GetCurrentAsync()).Localization.Currency,
                  Notes = SanitizeNotes(model.Notes), // FIXED: XSS protection
                  CreatedAt = DateTime.UtcNow,
                  CreatedByUserId = createdByUserId
            };

            appointment.CalculateEndTime(service);

            try
            {
                  _unitOfWork.Appointments.Add(appointment);
                  await _unitOfWork.SaveChangesAsync();

                  // Update customer stats
                  await UpdateCustomerBookingCountAsync(model.CustomerId);
            }
            catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
            {
                  _logger.LogWarning(
                      "Double appointment prevented. ServiceId: {ServiceId}, Date: {Date}, Time: {Time}",
                      model.ServiceId, model.AppointmentDate, model.StartTime);

                  throw new DomainException(_localizationService["Appointment_AlreadyBooked"], exception);
            }

            _logger.LogInformation(
                "Created appointment {AppointmentId} for customer {CustomerId}",
                appointment.Id, model.CustomerId);

            return appointment;
      }

      public async Task<Appointment> UpdateAppointmentAsync(int id, AppointmentRequestViewModel model)
      {
            var appointment = await _unitOfWork.Appointments.FindFirstOrDefaultAsync(predicate: b => b.Id == id && !b.IsDeleted, orderBy: x => x.Id);
            if (appointment == null || appointment.IsDeleted)
                  throw new DomainException(_localizationService["Appointment_NotFound"]);

            var isTimeChanged = appointment.AppointmentDate != model.AppointmentDate ||
                               appointment.StartTime != model.StartTime;

            if (isTimeChanged && !await _availabilityService.IsSlotAvailableAsync(
                model.ServiceId, model.AppointmentDate, model.StartTime, excludeBookingId: id))
            {
                  throw new DomainException(_localizationService["Appointment_Time_NotAvailable"]);
            }

            var service = await GetServiceOrThrowAsync(model.ServiceId);

            appointment.CustomerId = model.CustomerId;
            appointment.ServiceId = model.ServiceId;
            appointment.AppointmentDate = model.AppointmentDate;
            appointment.StartTime = model.StartTime;
            appointment.Status = model.Status;
            appointment.TotalPrice = service.Price;
            appointment.DiscountAmount = model.DiscountAmount;
            appointment.FinalPrice = CalculateFinalPrice(service.Price, model.DiscountAmount);
            appointment.Notes = SanitizeNotes(model.Notes);
            appointment.UpdatedAt = DateTime.UtcNow;

            appointment.CalculateEndTime(service);

            try
            {
                  _unitOfWork.Appointments.Update(appointment);
                  await _unitOfWork.SaveChangesAsync();


            }
            catch (DbUpdateConcurrencyException exception)
            {
                  _logger.LogWarning(
                      "Concurrency conflict on appointment {AppointmentId}. User: {User}",
                      id, model.CustomerId);

                  throw new DomainException(_localizationService["Appointment_UpdateConcurrency"], exception);
            }

            _logger.LogInformation("Appointment {AppointmentId} updated successfully", id);
            return appointment;
      }

      public async Task DeleteAppointmentAsync(int id)
      {
            var appointment = await _unitOfWork.Appointments.FindByIdAsync(id);
            if (appointment == null || appointment.IsDeleted)
                  throw new DomainException(_localizationService["Appointment_NotFound"]);

            if (appointment.Status == BookingStatus.Completed)
                  throw new DomainException(_localizationService["Appointment_Delete_CompletedNotAllowed"]);

            appointment.IsDeleted = true;
            appointment.DeletedAt = DateTime.UtcNow;

            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync();
            var customer = await _unitOfWork.Customers.FindByIdAsync(appointment.CustomerId);

            if ((customer?.Appointments ?? []).Count > 0)
            {
                  customer?.DecrementAppointments();
                  await _unitOfWork.SaveChangesAsync();
            }

            _logger.LogInformation("Soft-deleted appointment {AppointmentId}", id);
      }

      public async Task UpdateStatusAsync(int id, BookingStatus newStatus)
      {
            var appointment = await _unitOfWork.Appointments.FindFirstOrDefaultAsync(predicate: b => b.Id == id && !b.IsDeleted, orderBy: x => x.Id);
            if (appointment == null)
                  throw new DomainException(_localizationService["Appointment_NotFound"]);

            if (!IsValidStatusTransition(appointment.Status, newStatus))
                  throw new DomainException(_localizationService["Appointment_Status_InvalidTransition"]);

            appointment.Status = newStatus;
            appointment.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Changed appointment {AppointmentId} status from {OldStatus} to {NewStatus}",
                id, appointment.Status, newStatus);
      }

      public Task<bool> IsSlotAvailableAsync(int serviceId, DateTime date, TimeSpan startTime, int? excludeBookingId = null)
      {
            return _availabilityService.IsSlotAvailableAsync(serviceId, date, startTime, excludeBookingId);
      }

      // Private helpers

      /// <summary>
      /// Calculates the final price after applying discount (ensures non-negative result).
      /// </summary>
      private static decimal CalculateFinalPrice(decimal basePrice, decimal? discountAmount)
      {
            return Math.Max(0, basePrice - (discountAmount ?? 0));
      }

      /// <summary>
      /// Checks if a DbUpdateException is caused by a unique constraint violation (cross-database support: PostgreSQL, SQL Server).
      /// </summary>
      public static bool IsUniqueConstraintViolation(DbUpdateException exception)
      {
            if (exception.GetBaseException() is PostgresException postgresException)
            {
                  return postgresException.SqlState == "23505"; // Unique violation
            }

            if (exception.GetBaseException() is SqlException sqlException)
            {
                  return sqlException.Errors.Cast<SqlError>()
                      .Any(e => e.Number == 2601 || e.Number == 2627);
            }

            return false;
      }

      /// <summary>
      /// Retrieves a service by ID or throws a domain exception if not found or deleted.
      /// </summary>
      private async Task<Service> GetServiceOrThrowAsync(int serviceId)
      {
            var service = await _unitOfWork.Services.FindFirstOrDefaultAsync(predicate: b => b.Id == serviceId && !b.IsDeleted, orderBy: x => x.Id);
            if (service == null)
                  throw new DomainException(_localizationService.GetString("Appointment_ServiceNotFound", serviceId));
            return service;
      }

      /// <summary>
      /// Increments the appointment count for a customer and saves changes.
      /// </summary>
      private async Task UpdateCustomerBookingCountAsync(int customerId)
      {
            var customer = await _unitOfWork.Customers.FindByIdAsync(customerId);
            if (customer == null)
            {
                  _logger.LogWarning("Customer {CustomerId} not found", customerId);
                  return;
            }

            customer.IncrementAppointments();
            await _unitOfWork.SaveChangesAsync();
      }

      /// <summary>
      /// Sanitizes notes by escaping HTML characters to prevent XSS attacks.
      /// </summary>
      private static string? SanitizeNotes(string? notes)
      {
            if (string.IsNullOrEmpty(notes)) return notes;

            var sanitizer = new HtmlSanitizer();
            return sanitizer.Sanitize(notes);
      }

      /// <summary>
      /// Validates if a status transition from current to next is allowed based on business rules.
      /// </summary>
      private static bool IsValidStatusTransition(BookingStatus currentStatus, BookingStatus nextStatus)
      {
            return (currentStatus, nextStatus) switch
            {
                  (BookingStatus.Pending, BookingStatus.Confirmed) => true,
                  (BookingStatus.Pending, BookingStatus.Cancelled) => true,
                  (BookingStatus.Confirmed, BookingStatus.InProgress) => true,
                  (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,
                  (BookingStatus.InProgress, BookingStatus.Completed) => true,
                  (BookingStatus.InProgress, BookingStatus.Cancelled) => true,
                  (BookingStatus.Rescheduled, _) => true,
                  _ => false
            };
      }
}

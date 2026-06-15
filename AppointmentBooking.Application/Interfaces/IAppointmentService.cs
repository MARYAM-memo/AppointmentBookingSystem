using AppointmentBooking.Application.DTOs;
using AppointmentBooking.Application.ViewModels.Appointment;
using AppointmentBooking.Core.Models;

namespace AppointmentBooking.Application.Interfaces;

public interface IAppointmentService
{

      /// <summary>
      /// Retrieves filtered appointments based on provided filters with included navigation properties (Service and Customer).
      /// </summary>
      Task<IEnumerable<Appointment>?> GetFilteredAppointmentsAsync(AppointmentFiltersDTO filters);

      /// <summary>
      /// Creates a new appointment asynchronously after validating availability, calculates pricing, and handles concurrency conflicts.
      /// </summary>
      Task<Appointment> CreateAppointmentAsync(AppointmentRequestViewModel model, string? createdByUserId);

      /// <summary>
      /// Updates an existing appointment asynchronously, validates time changes for availability, and handles concurrency conflicts.
      /// </summary>
      Task<Appointment> UpdateAppointmentAsync(int id, AppointmentRequestViewModel model);


      /// <summary>
      /// Soft-deletes an appointment asynchronously (throws if appointment is completed) and updates customer appointment count.
      /// </summary>
      Task DeleteAppointmentAsync(int id);

      /// <summary>
      /// Updates the status of an appointment asynchronously after validating the status transition is allowed.
      /// </summary>
      Task UpdateStatusAsync(int id, BookingStatus newStatus);

      /// <summary>
      /// Checks if a specific time slot is available for appointment, optionally excluding a appointment ID for updates.
      /// </summary>
      Task<bool> IsSlotAvailableAsync(int serviceId, DateTime date, TimeSpan startTime, int? excludeBookingId = null);
}

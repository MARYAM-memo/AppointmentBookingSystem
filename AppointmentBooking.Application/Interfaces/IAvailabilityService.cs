using AppointmentBooking.Application.DTOs;

namespace AppointmentBooking.Application.Interfaces;

public interface IAvailabilityService
{
      /// <summary>
      /// Retrieves all available time slots for a service on a given date based on business hours, slot duration, and existing appointments.
      /// </summary>
      Task<List<TimeSlotDTO>> GetAvailableSlotsAsync(int serviceId, DateTime date, int? excludeAppointmentId = null);

      /// <summary>
      /// Checks if a specific time slot is available (overlap detection with existing non-cancelled appointments).
      /// </summary>
      Task<bool> IsSlotAvailableAsync(int serviceId, DateTime date, TimeSpan startTime, int? excludeBookingId = null);
}

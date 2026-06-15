using AppointmentBooking.Application.DTOs;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Core.Interfaces;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Application.Shared;

namespace AppointmentBooking.Infrastructure.Services;

public class AvailabilityService(IUnitOfWork unitOfWork, IBusinessProfileService businessProfile) : IAvailabilityService
{
      readonly IUnitOfWork _unitOfWork = unitOfWork;
      readonly IBusinessProfileService _businessProfile = businessProfile;

      public async Task<List<TimeSlotDTO>> GetAvailableSlotsAsync(int serviceId, DateTime date, int? excludeAppointmentId = null)
      {
            var service = await _unitOfWork.Services.FindByIdAsync(serviceId);
            if (service == null) return [];

            var profile = await _businessProfile.GetCurrentAsync();
            var slotDuration = TimeSpan.FromMinutes(profile?.SlotDurationMinutes ?? Constants.Defaults.SlotDurationMinutes);
            var workingHoursStart = profile?.WorkingHoursStart ?? TimeSpan.FromHours(Constants.Defaults.workingHoursStart);
            var workingHoursEnd = profile?.WorkingHoursEnd ?? TimeSpan.FromHours(Constants.Defaults.workingHoursEnd);

            // Get ALL appointments for this date
            var allAppointments = await _unitOfWork.Appointments.FetchAsync(
                b => b.AppointmentDate.Date == date.Date &&
                b.ServiceId == serviceId &&
                !b.IsDeleted &&
                b.Status != BookingStatus.Cancelled,
                asNoTracking: true
            );

            var currentAppointment = excludeAppointmentId.HasValue
                ? allAppointments.FirstOrDefault(b => b.Id == excludeAppointmentId)
                : null;

            // Exclude the currentAppointment from all calculations - because the user will change it.
            var existingAppointments = allAppointments
                .Where(b => b.Id != excludeAppointmentId)
                .ToList();

            var availableSlots = new List<TimeSlotDTO>();
            var currentTimeSlot = workingHoursStart;

            var now = DateTime.Now;
            var isToday = date.Date == now.Date;
            var minAllowedTime = isToday ? now.AddMinutes(5).TimeOfDay : TimeSpan.Zero;

            var serviceTotalDuration = service.GetTotalDuration();

            while (currentTimeSlot < workingHoursEnd)
            {
                  var proposedEndTime = currentTimeSlot.Add(serviceTotalDuration);

                  if (proposedEndTime > workingHoursEnd)
                        break;

                  // Is this the current slot?
                  bool isCurrentAppointment = currentAppointment != null &&
                                              currentAppointment.StartTime == currentTimeSlot;

                  // Count overlapping of existing appointments only (excluding current ones)
                  var overlappingCount = existingAppointments.Count(b =>
                      currentTimeSlot < b.EndTime && proposedEndTime > b.StartTime);

                  // Check capacity
                  bool isFullyBooked = overlappingCount >= service.MaxCapacity;

                  // Check past time
                  bool isPastSlot = isToday && currentTimeSlot < minAllowedTime;

                  // Available if: not fully booked AND not past OR it's the current appointment's slot
                  bool isAvailable = isCurrentAppointment || (!isFullyBooked && !isPastSlot);

                  availableSlots.Add(new TimeSlotDTO
                  {
                        StartTime = currentTimeSlot,
                        EndTime = proposedEndTime,
                        IsAvailable = isAvailable,
                        IsPastSlot = isPastSlot,
                        IsCurrentAppointment = isCurrentAppointment,
                        BookedCount = overlappingCount,
                        MaxCapacity = service.MaxCapacity,
                        DisplayText = $"{currentTimeSlot:hh\\:mm} - {proposedEndTime:hh\\:mm}"
                  });

                  currentTimeSlot = currentTimeSlot.Add(slotDuration);
            }

            return availableSlots;
      }

      public async Task<bool> IsSlotAvailableAsync(int serviceId, DateTime date, TimeSpan startTime, int? excludeAppointmentId = null)
      {
            var service = await _unitOfWork.Services.FindByIdAsync(serviceId);
            if (service == null) return false;

            var endTime = startTime.Add(service.GetTotalDuration());

            var allAppointments = await _unitOfWork.Appointments.FetchAsync(
                b => b.AppointmentDate.Date == date.Date &&
                     b.ServiceId == serviceId &&
                     !b.IsDeleted &&
                     b.Status != BookingStatus.Cancelled,
                asNoTracking: true
            );

            var currentAppointment = excludeAppointmentId.HasValue
                ? allAppointments.FirstOrDefault(b => b.Id == excludeAppointmentId)
                : null;

            // Same slot in edit mode - allow (the user does not change anything)
            if (currentAppointment != null &&
                currentAppointment.StartTime == startTime &&
                currentAppointment.EndTime == endTime)
            {
                  return true;
            }

            // Exclude current appointment completely - because the user will change it
            var existingAppointments = allAppointments.Where(b => b.Id != excludeAppointmentId);

            var overlappingCount = existingAppointments.Count(b =>
                b.StartTime < endTime && b.EndTime > startTime);

            return overlappingCount < service.MaxCapacity;
      }
}

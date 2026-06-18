using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Infrastructure.Data;
using AppointmentBooking.Infrastructure.DataAccess;
using AppointmentBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AppointmentBooking.Tests.Services;

public class AvailabilityServiceTests : IDisposable
{
      private readonly DatabaseContext _context;
      private readonly Mock<IBusinessProfileService> _businessProfileMock;
      private readonly AvailabilityService _service;
      private readonly Service _testService;

      public AvailabilityServiceTests()
      {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DatabaseContext(options);
            _businessProfileMock = new Mock<IBusinessProfileService>();
            _service = new AvailabilityService(
                new UnitOfWork(_context),
                _businessProfileMock.Object
            );

            _testService = new Service
            {
                  Id = 1,
                  Name = "Haircut",
                  Price = 50,
                  Duration = TimeSpan.FromMinutes(60),
                  BufferBefore = TimeSpan.FromMinutes(15),
                  BufferAfter = TimeSpan.FromMinutes(15),
                  MaxCapacity = 1,
                  IsActive = true
            };

            _context.Services.Add(_testService);
            _context.SaveChanges();

            // Default business profile
            _businessProfileMock
                .Setup(x => x.GetCurrentAsync())
                .ReturnsAsync(new BusinessProfile
                {
                      WorkingHoursStart = TimeSpan.FromHours(9),
                      WorkingHoursEnd = TimeSpan.FromHours(18),
                      SlotDurationMinutes = 30
                });
      }

      public void Dispose()
      {
            _context.Database.EnsureDeleted();
            _context.Dispose();
      }

      private Appointment CreateAppointment(int id, int customerId, DateTime date, TimeSpan startTime, BookingStatus status = BookingStatus.Confirmed)
      {
            var endTime = startTime.Add(_testService.Duration).Add(_testService.BufferAfter ?? TimeSpan.Zero);
            return new Appointment
            {
                  Id = id,
                  CustomerId = customerId,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime,
                  EndTime = endTime,
                  Status = status,
                  TotalPrice = 50,
                  FinalPrice = 50,
                  Currency = "EGP",
                  CreatedAt = DateTime.UtcNow
            };
      }

      // ============================================
      // IS SLOT AVAILABLE TESTS
      // ============================================

      [Fact]
      public async Task IsSlotAvailableAsync_EmptySchedule_ReturnsTrue()
      {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            // Act
            var result = await _service.IsSlotAvailableAsync(_testService.Id, date, startTime);

            // Assert
            Assert.True(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_SameTimeAsExisting_ReturnsFalse()
      {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            var existing = CreateAppointment(1, 1, date, startTime);
            _context.Appointments.Add(existing);
            _context.SaveChanges();

            // Act
            var result = await _service.IsSlotAvailableAsync(_testService.Id, date, startTime);

            // Assert
            Assert.False(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_OverlappingStart_ReturnsFalse()
      {
            // Arrange - Existing: 10:00-11:30, New: 10:30-12:00 (overlaps)
            var date = DateTime.Today.AddDays(1);
            var existingStart = TimeSpan.FromHours(10);
            var newStart = TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(30));

            var existing = CreateAppointment(1, 1, date, existingStart);
            _context.Appointments.Add(existing);
            _context.SaveChanges();

            // Act
            var result = await _service.IsSlotAvailableAsync(_testService.Id, date, newStart);

            // Assert
            Assert.False(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_OverlappingEnd_ReturnsFalse()
      {
            // Arrange - Existing: 10:00-11:30, New: 09:00-10:30 (overlaps at end)
            var date = DateTime.Today.AddDays(1);
            var existingStart = TimeSpan.FromHours(10);
            var newStart = TimeSpan.FromHours(9);

            var existing = CreateAppointment(1, 1, date, existingStart);
            _context.Appointments.Add(existing);
            _context.SaveChanges();

            // Act
            var result = await _service.IsSlotAvailableAsync(_testService.Id, date, newStart);

            // Assert
            Assert.False(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_CompletelyInsideExisting_ReturnsFalse()
      {
            // Arrange - Existing: 10:00-11:30, New: 10:15-11:15 (inside)
            var date = DateTime.Today.AddDays(1);
            var existingStart = TimeSpan.FromHours(10);
            var newStart = TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(15));

            var existing = CreateAppointment(1, 1, date, existingStart);
            _context.Appointments.Add(existing);
            _context.SaveChanges();

            // Act
            var result = await _service.IsSlotAvailableAsync(_testService.Id, date, newStart);

            // Assert
            Assert.False(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_ExistingCompletelyInsideNew_ReturnsFalse()
      {
            // Arrange - Existing: 10:00-11:30, New: 09:00-13:00 (wraps around)
            var date = DateTime.Today.AddDays(1);
            var existingStart = TimeSpan.FromHours(10);
            var newStart = TimeSpan.FromHours(9);

            // Need a longer service for this test
            var longService = new Service
            {
                  Id = 2,
                  Name = "Long Service",
                  Price = 100,
                  Duration = TimeSpan.FromHours(4),
                  MaxCapacity = 1,
                  IsActive = true
            };
            _context.Services.Add(longService);
            _context.SaveChanges();

            var existing = CreateAppointment(1, 1, date, existingStart);
            existing.ServiceId = longService.Id;
            existing.EndTime = existingStart.Add(longService.Duration);
            _context.Appointments.Add(existing);
            _context.SaveChanges();

            // Act - Try to book the long service at 9:00 (wraps 10:00-11:30)
            var result = await _service.IsSlotAvailableAsync(longService.Id, date, newStart);

            // Assert
            Assert.False(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_NonOverlapping_ReturnsTrue()
      {
            // Arrange - Existing: 10:00-11:30, New: 12:00-13:30 (no overlap)
            var date = DateTime.Today.AddDays(1);
            var existingStart = TimeSpan.FromHours(10);
            var newStart = TimeSpan.FromHours(12);

            var existing = CreateAppointment(1, 1, date, existingStart);
            _context.Appointments.Add(existing);
            _context.SaveChanges();

            // Act
            var result = await _service.IsSlotAvailableAsync(_testService.Id, date, newStart);

            // Assert
            Assert.True(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_TouchingEndToStart_ReturnsTrue()
      {
            // Arrange - Existing: 10:00-11:30, New: 11:30-13:00 (exactly touching)
            var date = DateTime.Today.AddDays(1);
            var existingStart = TimeSpan.FromHours(10);
            var newStart = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30));

            var existing = CreateAppointment(1, 1, date, existingStart);
            _context.Appointments.Add(existing);
            _context.SaveChanges();

            // Act
            var result = await _service.IsSlotAvailableAsync(_testService.Id, date, newStart);

            // Assert - Should be allowed (end time == start time, no overlap)
            Assert.True(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_ExcludeCurrentAppointment_ReturnsTrue()
      {
            // Arrange - Editing existing appointment, same time should be allowed
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            var existing = CreateAppointment(1, 1, date, startTime);
            _context.Appointments.Add(existing);
            _context.SaveChanges();

            // Act - Exclude the same appointment (edit mode)
            var result = await _service.IsSlotAvailableAsync(_testService.Id, date, startTime, excludeAppointmentId: 1);

            // Assert
            Assert.True(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_CancelledAppointment_Ignored()
      {
            // Arrange - Cancelled appointment should not block
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            var cancelled = CreateAppointment(1, 1, date, startTime, BookingStatus.Cancelled);
            _context.Appointments.Add(cancelled);
            _context.SaveChanges();

            // Act
            var result = await _service.IsSlotAvailableAsync(_testService.Id, date, startTime);

            // Assert
            Assert.True(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_NoShowAppointment_Ignored()
      {
            // Arrange - NoShow appointment should not block
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            var noShow = CreateAppointment(1, 1, date, startTime, BookingStatus.NoShow);
            _context.Appointments.Add(noShow);
            _context.SaveChanges();

            // Act
            var result = await _service.IsSlotAvailableAsync(_testService.Id, date, startTime);

            // Assert
            Assert.True(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_DifferentService_ReturnsTrue()
      {
            // Arrange - Different service should not conflict
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            var service2 = new Service
            {
                  Id = 2,
                  Name = "Nails",
                  Price = 30,
                  Duration = TimeSpan.FromMinutes(45),
                  MaxCapacity = 1,
                  IsActive = true
            };
            _context.Services.Add(service2);

            var existing = CreateAppointment(1, 1, date, startTime);
            _context.Appointments.Add(existing);
            _context.SaveChanges();

            // Act - Book service2 at same time
            var result = await _service.IsSlotAvailableAsync(service2.Id, date, startTime);

            // Assert
            Assert.True(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_DifferentDate_ReturnsTrue()
      {
            // Arrange - Different date should not conflict
            var date1 = DateTime.Today.AddDays(1);
            var date2 = DateTime.Today.AddDays(2);
            var startTime = TimeSpan.FromHours(10);

            var existing = CreateAppointment(1, 1, date1, startTime);
            _context.Appointments.Add(existing);
            _context.SaveChanges();

            // Act
            var result = await _service.IsSlotAvailableAsync(_testService.Id, date2, startTime);

            // Assert
            Assert.True(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_MaxCapacityReached_ReturnsFalse()
      {
            // Arrange - Service with capacity 2, 2 bookings already
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            var multiService = new Service
            {
                  Id = 3,
                  Name = "Group Class",
                  Price = 20,
                  Duration = TimeSpan.FromMinutes(60),
                  MaxCapacity = 2,
                  IsActive = true
            };
            _context.Services.Add(multiService);
            _context.SaveChanges();

            var appt1 = CreateAppointment(1, 1, date, startTime);
            appt1.ServiceId = multiService.Id;
            var appt2 = CreateAppointment(2, 2, date, startTime);
            appt2.ServiceId = multiService.Id;

            _context.Appointments.AddRange(appt1, appt2);
            _context.SaveChanges();

            // Act - Try 3rd booking
            var result = await _service.IsSlotAvailableAsync(multiService.Id, date, startTime);

            // Assert
            Assert.False(result);
      }

      [Fact]
      public async Task IsSlotAvailableAsync_CapacityNotReached_ReturnsTrue()
      {
            // Arrange - Capacity 2, only 1 booking
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            var multiService = new Service
            {
                  Id = 3,
                  Name = "Group Class",
                  Price = 20,
                  Duration = TimeSpan.FromMinutes(60),
                  MaxCapacity = 2,
                  IsActive = true
            };
            _context.Services.Add(multiService);
            _context.SaveChanges();

            var appt1 = CreateAppointment(1, 1, date, startTime);
            appt1.ServiceId = multiService.Id;
            _context.Appointments.Add(appt1);
            _context.SaveChanges();

            // Act - 2nd booking should be allowed
            var result = await _service.IsSlotAvailableAsync(multiService.Id, date, startTime);

            // Assert
            Assert.True(result);
      }

      // ============================================
      // GET AVAILABLE SLOTS TESTS
      // ============================================

      [Fact]
      public async Task GetAvailableSlotsAsync_ReturnsCorrectSlots()
      {
            // Arrange
            var date = DateTime.Today.AddDays(1);

            // Act
            var slots = await _service.GetAvailableSlotsAsync(_testService.Id, date);

            // Assert
            Assert.NotEmpty(slots);
            Assert.All(slots, slot =>
            {
                  Assert.True(slot.StartTime >= TimeSpan.FromHours(9));
                  Assert.True(slot.EndTime <= TimeSpan.FromHours(18));
            });
      }

      [Fact]
      public async Task GetAvailableSlotsAsync_WithExistingAppointment_MarksOccupied()
      {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            var existing = CreateAppointment(1, 1, date, startTime);
            _context.Appointments.Add(existing);
            _context.SaveChanges();

            // Act
            var slots = await _service.GetAvailableSlotsAsync(_testService.Id, date);

            // Assert
            var occupiedSlot = slots.FirstOrDefault(s => s.StartTime == startTime);
            Assert.NotNull(occupiedSlot);
            Assert.False(occupiedSlot.IsAvailable);
            Assert.Equal(1, occupiedSlot.BookedCount);
      }

      [Fact]
      public async Task GetAvailableSlotsAsync_ExcludeCurrentAppointment_MarksAvailable()
      {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            var existing = CreateAppointment(1, 1, date, startTime);
            _context.Appointments.Add(existing);
            _context.SaveChanges();

            // Act - Edit mode
            var slots = await _service.GetAvailableSlotsAsync(_testService.Id, date, excludeAppointmentId: 1);

            // Assert
            var currentSlot = slots.FirstOrDefault(s => s.StartTime == startTime);
            Assert.NotNull(currentSlot);
            Assert.True(currentSlot.IsAvailable); // Should be available for editing
            Assert.True(currentSlot.IsCurrentAppointment);
      }

      [Fact]
      public async Task GetAvailableSlotsAsync_PastSlots_MarkedAsPast()
      {
            // Arrange - Today with past time
            var date = DateTime.Today;
            var now = DateTime.Now;

            // Act
            var slots = await _service.GetAvailableSlotsAsync(_testService.Id, date);

            // Assert
            var pastSlots = slots.Where(s => s.StartTime < now.TimeOfDay);
            Assert.All(pastSlots, slot => Assert.True(slot.IsPastSlot));
      }
}

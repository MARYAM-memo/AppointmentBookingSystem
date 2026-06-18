using AppointmentBooking.Application.Exceptions;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.Appointment;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Infrastructure.Data;
using AppointmentBooking.Infrastructure.DataAccess;
using AppointmentBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppointmentBooking.Tests.Services;

public class AppointmentServiceTests : IDisposable
{
      private readonly DatabaseContext _context;
      private readonly Mock<IAvailabilityService> _availabilityServiceMock;
      private readonly Mock<IBusinessProfileService> _businessProfileMock;
      private readonly Mock<ILocalizationService> _localizerMock;
      private readonly AppointmentService _service;
      private readonly Service _testService;
      private readonly Customer _testCustomer;

      public AppointmentServiceTests()
      {
            try
            {
                  // In-memory database for isolation
                  var options = new DbContextOptionsBuilder<DatabaseContext>()
                      .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                      .Options;

                  _context = new DatabaseContext(options);
                  _availabilityServiceMock = new Mock<IAvailabilityService>();
                  _localizerMock = new Mock<ILocalizationService>();
                  _businessProfileMock = new Mock<IBusinessProfileService>();

                  // Setup localizer to return key as value
                  _localizerMock.Setup(x => x[It.IsAny<string>()])
                      .Returns((string key) => new LocalizedString(key, key));
                  _localizerMock.Setup(x => x.GetString(It.IsAny<string>(), It.IsAny<object[]>()))
                      .Returns((string key, object[] args) => key);

                  _businessProfileMock.Setup(x => x.GetCurrentAsync())
                          .ReturnsAsync(new BusinessProfile
                          {
                                Localization = new LocalizationConfig
                                {
                                      Currency = "EGP",
                                      Language = "ar",
                                      Direction = "rtl"
                                }
                          });

                  // IMPORTANT: Create UnitOfWork with the context
                  var unitOfWork = new UnitOfWork(_context);

                  _service = new AppointmentService(
                      unitOfWork,
                      _availabilityServiceMock.Object,
                      Mock.Of<ILogger<AppointmentService>>(),
                      _businessProfileMock.Object,
                      _localizerMock.Object
                  );

                  // Seed test data
                  _testService = new Service
                  {
                        Id = 1,
                        Name = "Test Service",
                        Price = 100,
                        Duration = TimeSpan.FromMinutes(60),
                        BufferBefore = TimeSpan.FromMinutes(15),
                        BufferAfter = TimeSpan.FromMinutes(15),
                        MaxCapacity = 1,
                        IsActive = true
                  };

                  _testCustomer = new Customer
                  {
                        Id = 1,
                        FullName = "Test Customer",
                        PhoneNumber = "0500000000",
                        CreatedAt = DateTime.UtcNow
                  };

                  _context.Services.Add(_testService);
                  _context.Customers.Add(_testCustomer);
                  _context.SaveChanges();
            }
            catch (Exception ex)
            {
                  // This will help you see the actual error
                  Console.WriteLine($"Constructor error: {ex.Message}");
                  Console.WriteLine($"Stack trace: {ex.StackTrace}");
                  throw;
            }
      }
      public void Dispose()
      {
            _context.Database.EnsureDeleted();
            _context.Dispose();
      }

      // ============================================
      // CREATE TESTS
      // ============================================

      [Fact]
      public async Task CreateAppointmentAsync_SlotAvailable_CreatesSuccessfully()
      {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime, null))
                .ReturnsAsync(true);

            var model = new AppointmentRequestViewModel
            {
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime,
                  Status = BookingStatus.Pending,
                  TotalPrice = 100,
                  FinalPrice = 100
            };

            // Act
            var result = await _service.CreateAppointmentAsync(model, "test-user");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testCustomer.Id, result.CustomerId);
            Assert.Equal(_testService.Id, result.ServiceId);
            Assert.Equal(date.Date, result.AppointmentDate.Date);
            Assert.Equal(startTime, result.StartTime);
            // Changed from 11:00 to 11:30 because of BufferBefore (15min) + Duration (60min) + BufferAfter (15min)
            Assert.Equal(TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30)), result.EndTime);
            _availabilityServiceMock.Verify(x => x.IsSlotAvailableAsync(
                _testService.Id, date, startTime, null), Times.Once);
      }

      [Fact]
      public async Task CreateAppointmentAsync_SlotNotAvailable_ThrowsDomainException()
      {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime, null))
                .ReturnsAsync(false);

            var model = new AppointmentRequestViewModel
            {
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime,
                  Status = BookingStatus.Pending
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.CreateAppointmentAsync(model, "test-user"));

            Assert.Contains("NotAvailable", exception.Message);
      }

      [Fact]
      public async Task CreateAppointmentAsync_DoubleBookingSameTime_ThrowsDomainException()
      {
            // Arrange - Create first appointment
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime, null))
                .ReturnsAsync(true);

            var model1 = new AppointmentRequestViewModel
            {
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime,
                  Status = BookingStatus.Pending,
                  TotalPrice = 100,
                  FinalPrice = 100
            };

            await _service.CreateAppointmentAsync(model1, "test-user");

            // Now simulate second booking at same time (availability returns false)
            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime, null))
                .ReturnsAsync(false);

            var model2 = new AppointmentRequestViewModel
            {
                  CustomerId = 2, // Different customer
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime,
                  Status = BookingStatus.Pending
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.CreateAppointmentAsync(model2, "test-user"));

            Assert.Contains("NotAvailable", exception.Message);
      }

      [Fact]
      public async Task CreateAppointmentAsync_OverlappingTime_ThrowsDomainException()
      {
            // Arrange - Create first appointment at 10:00-11:30 (60min + 15min buffer)
            var date = DateTime.Today.AddDays(1);
            var startTime1 = TimeSpan.FromHours(10);

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime1, null))
                .ReturnsAsync(true);

            var model1 = new AppointmentRequestViewModel
            {
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime1,
                  Status = BookingStatus.Pending,
                  TotalPrice = 100,
                  FinalPrice = 100
            };

            await _service.CreateAppointmentAsync(model1, "test-user");

            // Try to book overlapping time (10:30 - inside first appointment)
            var startTime2 = TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(30));

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime2, null))
                .ReturnsAsync(false);

            var model2 = new AppointmentRequestViewModel
            {
                  CustomerId = 2,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime2,
                  Status = BookingStatus.Pending
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.CreateAppointmentAsync(model2, "test-user"));

            Assert.Contains("NotAvailable", exception.Message);
      }

      [Fact]
      public async Task CreateAppointmentAsync_AdjacentTime_Allowed()
      {
            // Arrange - First appointment 10:00-11:30
            var date = DateTime.Today.AddDays(1);
            var startTime1 = TimeSpan.FromHours(10);

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime1, null))
                .ReturnsAsync(true);

            var model1 = new AppointmentRequestViewModel
            {
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime1,
                  Status = BookingStatus.Pending,
                  TotalPrice = 100,
                  FinalPrice = 100
            };

            await _service.CreateAppointmentAsync(model1, "test-user");

            // Book at 11:30 (exactly after first ends) - should be allowed
            var startTime2 = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30));

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime2, null))
                .ReturnsAsync(true);

            var model2 = new AppointmentRequestViewModel
            {
                  CustomerId = 2,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime2,
                  Status = BookingStatus.Pending,
                  TotalPrice = 100,
                  FinalPrice = 100
            };

            // Act
            var result = await _service.CreateAppointmentAsync(model2, "test-user");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(startTime2, result.StartTime);
      }

      // ============================================
      // EDIT TESTS
      // ============================================

      [Fact]
      public async Task UpdateAppointmentAsync_ValidEdit_UpdatesSuccessfully()
      {
            // Arrange - Create initial appointment
            var date = DateTime.Today.AddDays(1);
            var originalStart = TimeSpan.FromHours(10);

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, originalStart, null))
                .ReturnsAsync(true);

            var createModel = new AppointmentRequestViewModel
            {
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = originalStart,
                  Status = BookingStatus.Pending,
                  TotalPrice = 100,
                  FinalPrice = 100
            };

            var created = await _service.CreateAppointmentAsync(createModel, "test-user");


            // Edit to new time
            var newStart = TimeSpan.FromHours(14);

            // IMPORTANT: Change the service price before editing
            _testService.Price = 150;
            _context.Services.Update(_testService);
            await _context.SaveChangesAsync();

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, newStart, created.Id))
                .ReturnsAsync(true);

            var editModel = new AppointmentRequestViewModel
            {
                  Id = created.Id,
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = newStart,
                  Status = BookingStatus.Confirmed,
                  TotalPrice = 150,
                  FinalPrice = 150
            };

            // Act
            var result = await _service.UpdateAppointmentAsync(created.Id, editModel);

            // Assert
            Assert.Equal(newStart, result.StartTime);
            Assert.Equal(BookingStatus.Confirmed, result.Status);
            Assert.Equal(150, result.TotalPrice);
      }

      [Fact]
      public async Task UpdateAppointmentAsync_SameTimeNoChange_Allowed()
      {
            // Arrange - Create appointment
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime, null))
                .ReturnsAsync(true);

            var createModel = new AppointmentRequestViewModel
            {
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime,
                  Status = BookingStatus.Pending,
                  TotalPrice = 100,
                  FinalPrice = 100
            };

            var created = await _service.CreateAppointmentAsync(createModel, "test-user");

            // Edit with same time (only status changes)
            var editModel = new AppointmentRequestViewModel
            {
                  Id = created.Id,
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime, // Same time
                  Status = BookingStatus.Confirmed, // Only status changed
                  TotalPrice = 100,
                  FinalPrice = 100
            };

            // Availability service should NOT be called for same time
            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime, created.Id))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAppointmentAsync(created.Id, editModel);

            // Assert
            Assert.Equal(startTime, result.StartTime);
            Assert.Equal(BookingStatus.Confirmed, result.Status);
      }

      [Fact]
      public async Task UpdateAppointmentAsync_ToOccupiedSlot_ThrowsDomainException()
      {
            // Arrange - Create two appointments
            var date = DateTime.Today.AddDays(1);
            var startTime1 = TimeSpan.FromHours(10);
            var startTime2 = TimeSpan.FromHours(14);

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime1, null))
                .ReturnsAsync(true);
            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime2, null))
                .ReturnsAsync(true);

            var model1 = new AppointmentRequestViewModel
            {
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime1,
                  Status = BookingStatus.Pending,
                  TotalPrice = 100,
                  FinalPrice = 100
            };
            var appt1 = await _service.CreateAppointmentAsync(model1, "test-user");

            var model2 = new AppointmentRequestViewModel
            {
                  CustomerId = 2,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime2,
                  Status = BookingStatus.Pending,
                  TotalPrice = 100,
                  FinalPrice = 100
            };
            await _service.CreateAppointmentAsync(model2, "test-user");

            // Try to move appt1 to appt2's time
            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime2, appt1.Id))
                .ReturnsAsync(false);

            var editModel = new AppointmentRequestViewModel
            {
                  Id = appt1.Id,
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime2, // Occupied by appt2
                  Status = BookingStatus.Pending,
                  TotalPrice = 100,
                  FinalPrice = 100
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.UpdateAppointmentAsync(appt1.Id, editModel));

            Assert.Contains("NotAvailable", exception.Message);
      }

      [Fact]
      public async Task UpdateAppointmentAsync_NonExistent_ThrowsDomainException()
      {
            // Arrange
            var model = new AppointmentRequestViewModel
            {
                  Id = 999,
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = DateTime.Today.AddDays(1),
                  StartTime = TimeSpan.FromHours(10),
                  Status = BookingStatus.Pending
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.UpdateAppointmentAsync(999, model));

            Assert.Contains("NotFound", exception.Message);
      }

      [Fact]
      public async Task UpdateAppointmentAsync_DeletedAppointment_ThrowsDomainException()
      {
            // Arrange - Create and soft-delete
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime, null))
                .ReturnsAsync(true);

            var createModel = new AppointmentRequestViewModel
            {
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime,
                  Status = BookingStatus.Pending,
                  TotalPrice = 100,
                  FinalPrice = 100
            };

            var created = await _service.CreateAppointmentAsync(createModel, "test-user");
            await _service.DeleteAppointmentAsync(created.Id);

            var editModel = new AppointmentRequestViewModel
            {
                  Id = created.Id,
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = TimeSpan.FromHours(14),
                  Status = BookingStatus.Pending
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.UpdateAppointmentAsync(created.Id, editModel));

            Assert.Contains("NotFound", exception.Message);
      }

      // ============================================
      // EDGE CASES
      // ============================================

      [Fact]
      public async Task CreateAppointmentAsync_PastDate_ThrowsDomainException()
      {
            // This is validated in the validator, but service should also handle
            var model = new AppointmentRequestViewModel
            {
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = DateTime.Today.AddDays(-1), // Past
                  StartTime = TimeSpan.FromHours(10),
                  Status = BookingStatus.Pending
            };

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), null))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.CreateAppointmentAsync(model, "test-user"));

            // The validator catches this, but if bypassed:
            Assert.Contains("Appointment_Date_Past", exception.Message);
      }

      [Fact]
      public async Task CreateAppointmentAsync_ZeroPrice_Allowed()
      {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var startTime = TimeSpan.FromHours(10);

            _availabilityServiceMock
                .Setup(x => x.IsSlotAvailableAsync(_testService.Id, date, startTime, null))
                .ReturnsAsync(true);

            var model = new AppointmentRequestViewModel
            {
                  CustomerId = _testCustomer.Id,
                  ServiceId = _testService.Id,
                  AppointmentDate = date,
                  StartTime = startTime,
                  Status = BookingStatus.Pending,
                  TotalPrice = 0,
                  FinalPrice = 0
            };

            // Act
            var result = await _service.CreateAppointmentAsync(model, "test-user");

            // Assert
            Assert.Equal(0, result.TotalPrice);
            Assert.Equal(0, result.FinalPrice);
      }
}

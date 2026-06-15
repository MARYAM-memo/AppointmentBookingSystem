using AppointmentBooking.Core.Models;

namespace AppointmentBooking.Tests.PaginationTest;

public class AppointmentPaginationTests
{
      [Fact]
      public void Test_Appointment_Pagination_With_Mock_Data()
      {
            // Arrange - Fake Data
            var mockAppointments = CreateMockAppointments(60); // 60 mock appointment

            int pageSize = 25;
            int pageNumber = 2; //Second page

            // Act - Applying Pagination
            var pagedResult = mockAppointments
                .OrderByDescending(a => a.AppointmentDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Assert
            Assert.Equal(25, pagedResult.Count);// The second page must contain 25 items
            Assert.All(pagedResult, item => Assert.NotNull(item));

            // Check for duplicates
            var distinctIds = pagedResult.Select(a => a.Id).Distinct();
            Assert.Equal(pagedResult.Count, distinctIds.Count());
      }

      [Fact]
      public void Test_Appointment_Last_Page_With_Remaining_Items()
      {
            // Arrange
            var mockAppointments = CreateMockAppointments(55); // 55 mock appointment
            int pageSize = 25;
            int lastPageNumber = 3; // 55/25 = 3 pages (the last page has 5 items)

            // Act
            var lastPageItems = mockAppointments
                .Skip((lastPageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Assert
            Assert.Equal(5, lastPageItems.Count); // The last page should contain only 5 items
      }

      [Theory]
      [InlineData(1, 25, 60, 25)]  // Page 1: 25 items
      [InlineData(2, 25, 60, 25)]  // Page 2: 25 items
      [InlineData(3, 25, 60, 10)]  // Page 3: 10 items
      [InlineData(1, 10, 35, 10)]  // Page 1 (size 10): 10 items
      [InlineData(4, 10, 35, 5)]   // Page 4 (size 10): 5 items
      public void Test_Appointment_Pagination_Amounts(int pageNumber, int pageSize, int totalItems, int expectedItemsInPage)
      {
            // Arrange
            var mockAppointments = CreateMockAppointments(totalItems);

            // Act
            var pagedItems = mockAppointments
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Assert
            Assert.Equal(expectedItemsInPage, pagedItems.Count);
      }

      [Fact]
      public void Test_Pagination_Performance_With_Large_Data()
      {
            // 10000 item
            var largeData = CreateMockAppointments(10000);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var result = largeData.Skip(5000).Take(25).ToList();

            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 100,
                "Pagination should be fast even with large data");
      }

      // Helper method to create mock appointments
      private static List<Appointment> CreateMockAppointments(int count)
      {
            var appointments = new List<Appointment>();
            for (int i = 1; i <= count; i++)
            {
                  appointments.Add(new Appointment
                  {
                        Id = i,
                        AppointmentDate = DateTime.Today.AddDays(-(i % 30)),
                        StartTime = TimeSpan.FromHours(9 + (i % 8)),
                        EndTime = TimeSpan.FromHours(10 + (i % 8)),
                        Status = (BookingStatus)(i % 7),
                        Customer = new Customer
                        {
                              Id = i,
                              FullName = $"عميل {i}",
                              PhoneNumber = $"05{i:00000000}"
                        },
                        Service = new Service
                        {
                              Id = i % 5 + 1,
                              Name = $"خدمة {i % 5 + 1}"
                        }
                  });
            }
            return appointments;
      }

}

using AppointmentBooking.Tests.Helpers;
using FluentAssertions;

namespace AppointmentBooking.Tests.PaginationTest;

public class CustomerPaginationTests
{
      [Fact]
      public void Test_Customer_Pagination_With_Mock_Data()
      {
            // Arrange
            var mockCustomers = MockDataHelper.CreateMockCustomers(60); // 60 customer
            int pageSize = 25;
            int pageNumber = 2; //Second page

            // Act
            var pagedCustomers = mockCustomers
                      .OrderByDescending(c => c.CreatedAt)
                      .Skip((pageNumber - 1) * pageSize)
                      .Take(pageSize)
                      .ToList();

            // Assert
            Assert.Equal(25, pagedCustomers.Count); // Page 2, 25 items
            Assert.All(pagedCustomers, customer => Assert.NotNull(customer));

            // Check for duplicates
            var distinctIds = pagedCustomers.Select(c => c.Id).Distinct();
            Assert.Equal(pagedCustomers.Count, distinctIds.Count());
      }

      [Fact]
      public void Test_Customer_Last_Page_With_Remaining_Items()
      {
            // Arrange
            var mockCustomers = MockDataHelper.CreateMockCustomers(55); // 55 customer
            int pageSize = 25;
            int lastPageNumber = 3; // 55/25 = 3 pages (the last page has 5 items)

            // Act
            var lastPageItems = mockCustomers
                      .Skip((lastPageNumber - 1) * pageSize)
                      .Take(pageSize)
                      .ToList();

            // Assert
            Assert.Equal(5, lastPageItems.Count); // Last page: 5 customers
      }

      [Theory]
      [InlineData(1, 25, 60, 25)]  // Page 1: 25 clients
      [InlineData(2, 25, 60, 25)]  // Page 2: 25 clients
      [InlineData(3, 25, 60, 10)]  // Page 3: 10 clients
      [InlineData(1, 10, 35, 10)]  // Page 1 (size 10): 10 clientsء
      [InlineData(4, 10, 35, 5)]   // Page 4 (size 10): 5 clients
      public void Test_Customer_Pagination_Amounts(int pageNumber, int pageSize, int totalItems, int expectedItemsInPage)
      {
            // Arrange
            var mockCustomers = MockDataHelper.CreateMockCustomers(totalItems);

            // Act
            var pagedItems = mockCustomers
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Assert
            Assert.Equal(expectedItemsInPage, pagedItems.Count);
      }

      [Fact]
      public void Test_Customer_Sorting_With_Pagination()
      {
            // Arrange
            var mockCustomers = MockDataHelper.CreateMockCustomers(30);

            // Act - Sort by name then Pagination
            var pagedByName = mockCustomers
                      .OrderBy(c => c.FullName)
                      .Skip(0)
                      .Take(25)
                      .ToList();

            // Act - Arrangement based on the number of reservations
            var pagedByAppointments = mockCustomers
                      .OrderByDescending(c => c.TotalAppointments)
                      .Skip(0)
                      .Take(25)
                      .ToList();

            // Assert
            Assert.Equal(25, pagedByName.Count);
            Assert.Equal(25, pagedByAppointments.Count);

            // Check the order
            for (int i = 0; i < pagedByName.Count - 1; i++)
            {
                  Assert.True(string.Compare(pagedByName[i].FullName, pagedByName[i + 1].FullName) <= 0);
            }

            for (int i = 0; i < pagedByAppointments.Count - 1; i++)
            {
                  Assert.True(pagedByAppointments[i].TotalAppointments >= pagedByAppointments[i + 1].TotalAppointments);
            }
      }

      [Fact]
      public void Test_Customer_Filtering_With_Pagination()
      {
            // Arrange
            var mockCustomers = MockDataHelper.CreateMockCustomers(50);
            string searchTerm = "أحمد";

            // Act - Filter then Pagination
            var filtered = mockCustomers
                      .Where(c => c.FullName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
                      .ToList();

            var paged = filtered
                .Skip(0)
                .Take(25)
                .ToList();

            // Assert
            Assert.All(paged, c => Assert.Contains(searchTerm, c.FullName));
      }

      [Fact]
      public void Test_Customer_Pagination_TotalPages_Calculation()
      {
            // Arrange
            var testCases = new[]
            {
                (total: 0, pageSize: 25, expectedPages: 0),
                (total: 25, pageSize: 25, expectedPages: 1),
                (total: 26, pageSize: 25, expectedPages: 2),
                (total: 50, pageSize: 25, expectedPages: 2),
                (total: 51, pageSize: 25, expectedPages: 3),
                (total: 100, pageSize: 10, expectedPages: 10),
            };

            foreach (var (total, pageSize, expectedPages) in testCases)
            {
                  // Act
                  var calculatedPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

                  // Assert
                  Assert.Equal(expectedPages, calculatedPages);
            }
      }
}

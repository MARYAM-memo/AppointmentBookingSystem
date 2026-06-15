namespace AppointmentBooking.Tests.Helpers;

public class PaginationTester
{
      /// <summary>
      /// A basic test of all pagination properties
      /// </summary>
      public static void TestPaginationProperties<T>(int pageNumber, int pageSize, int totalCount, Action<int, int, int>? assertAction = null)
      {
            // Calculate
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var startItem = (pageNumber - 1) * pageSize + 1;
            var endItem = Math.Min(pageNumber * pageSize, totalCount);

            // Assertions
            Assert.True(pageNumber > 0, "PageNumber يجب أن يكون أكبر من 0");
            Assert.True(pageSize > 0, "PageSize يجب أن يكون أكبر من 0");

            if (totalCount > 0)
            {
                  Assert.True(totalPages >= 1, "يجب أن يكون عدد الصفحات 1 على الأقل");
                  Assert.True(startItem <= totalCount, "عنصر البداية يجب أن يكون ضمن النطاق");
                  Assert.True(endItem <= totalCount, "عنصر النهاية يجب أن يكون ضمن النطاق");
            }

            // Execute custom assertions if provided
            assertAction?.Invoke(startItem, endItem, totalPages);
      }

      /// <summary>
      /// Testing the absence of data
      /// </summary>
      public static void TestEmptyPagination()
      {
            var pageSize = 25;
            var totalCount = 0;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            Assert.Equal(0, totalPages);
      }

      /// <summary>
      /// Testing the boundary states of pages
      /// </summary>
      public static IEnumerable<object[]> GetBoundaryTestData()
      {
            yield return new object[] { 1, 25, 25, 1 };      // Exactly one page
            yield return new object[] { 1, 25, 26, 2 };      // 26 items -> Two pages
            yield return new object[] { 2, 25, 50, 2 };      // Exactly last page
            yield return new object[] { 2, 25, 51, 3 };      // Middle of pages
            yield return new object[] { 3, 25, 75, 3 };      // Last page
            yield return new object[] { 1, 10, 5, 1 };       // Items less than the page size
      }

      /// <summary>
      /// Page range testing (Previous/Next)
      /// </summary>
      public static void TestPaginationNavigation(int currentPage, int totalPages)
      {
            bool hasPrevious = currentPage > 1;
            bool hasNext = currentPage < totalPages;

            if (currentPage == 1)
            {
                  Assert.False(hasPrevious, "الصفحة الأولى يجب ألا تحتوي على زر السابق");
            }

            if (currentPage == totalPages)
            {
                  Assert.False(hasNext, "الصفحة الأخيرة يجب ألا تحتوي على زر التالي");
            }

            if (totalPages == 1)
            {
                  Assert.False(hasPrevious && hasNext, "صفحة واحدة يجب ألا تحتوي على أزرار تنقل");
            }
      }
}

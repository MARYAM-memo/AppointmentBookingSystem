using AppointmentBooking.Application.DTOs;

namespace AppointmentBooking.Application.Extensions;

public static class PaginationExtensions
{
      /// <summary>
      /// Converts an IEnumerable to a PagedResult with pagination metadata (page number, size, and total count).
      /// </summary>
      public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> query, int pageNumber, int pageSize)
      {
            var totalCount = query.Count();
            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<T>
            {
                  Items = items,
                  Pagination = new PaginationDTO
                  {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = totalCount
                  }
            };
      }

      /// <summary>
      /// Calculates the total number of pages based on total item count and page size (defaults to 25 if pageSize is invalid).
      /// </summary>
      public static int CalculateTotalPages(this int totalCount, int pageSize)
      {
            pageSize = pageSize > 0 ? pageSize : 25;
            return (int)Math.Ceiling(totalCount / (double)pageSize);
      }
}

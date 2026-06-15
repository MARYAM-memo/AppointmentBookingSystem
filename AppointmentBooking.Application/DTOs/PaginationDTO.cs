namespace AppointmentBooking.Application.DTOs;

public class PaginationDTO
{
      public int PageNumber { get; set; } = 1;
      public int PageSize { get; set; } = 25;
      public int TotalCount { get; set; }
      public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
      public bool HasPreviousPage => PageNumber > 1;
      public bool HasNextPage => PageNumber < TotalPages;
}

public class PagedResult<T>
{
      public List<T> Items { get; set; } = [];
      public PaginationDTO Pagination { get; set; } = new();
}
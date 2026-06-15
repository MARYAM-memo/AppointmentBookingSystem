namespace AppointmentBooking.Application.DTOs;

public class FilterDTO
{
      public string? SearchTerm { get; set; }
      public string? SortBy { get; set; }
      public int PageNumber { get; set; } = 1;
      public int PageSize { get; set; } = 25;
}
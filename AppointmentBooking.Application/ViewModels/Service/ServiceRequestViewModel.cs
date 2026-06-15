namespace AppointmentBooking.Application.ViewModels.Service;

public class ServiceRequestViewModel
{
      public int Id { get; set; }
      public string Name { get; set; } = string.Empty;
      public string? Description { get; set; }
      public string? Category { get; set; }
      public string? SubCategory { get; set; }
      public int DurationMinutes { get; set; } = 30;
      public decimal Price { get; set; }
      public decimal? PricePerAdditionalHour { get; set; }
      public int? BufferBeforeMinutes { get; set; }
      public int? BufferAfterMinutes { get; set; }
      public int MaxCapacity { get; set; } = 1;
      public string? RequiredDocuments { get; set; }
      public string? Icon { get; set; } = "bi-stars";
      public string? Color { get; set; } = "#2c6e7c";
      public string? ImageUrl { get; set; }
      public int DisplayOrder { get; set; }
      public bool IsActive { get; set; } = true;
      public bool IsPopular { get; set; }
}
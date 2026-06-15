namespace AppointmentBooking.Application.ViewModels.Service;

public class ServiceResponseViewModel
{
      public int Id { get; set; }
      public string Name { get; set; } = string.Empty;
      public string? Description { get; set; }
      public string? Category { get; set; }
      public string? SubCategory { get; set; }

      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? UpdatedAt { get; set; }
      public DateTime? DeletedAt { get; set; }
      public bool IsDeleted { get; set; }
      public int AppointmentsCount { get; set; }

      public string? Icon { get; set; } // Bootstrap icon class
      public string? Color { get; set; } // Custom color for the service
      public string? ImageUrl { get; set; }
      public int DisplayOrder { get; set; } = 0;
      public bool IsActive { get; set; } = true;
      public bool IsPopular { get; set; }

      public TimeSpan? BufferBefore { get; set; }
      public TimeSpan? BufferAfter { get; set; }
      public int MaxCapacity { get; set; } = 1;
      public TimeSpan Duration { get; set; }
      public TimeSpan TotalDuration { get; set; }
      public decimal Price { get; set; }
      public decimal? PricePerAdditionalHour { get; set; }
      public List<string>? RequiredDocuments { get; set; }
}

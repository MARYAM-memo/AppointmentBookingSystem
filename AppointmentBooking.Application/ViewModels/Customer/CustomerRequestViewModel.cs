namespace AppointmentBooking.Application.ViewModels.Customer;

public class CustomerRequestViewModel
{
      public int Id { get; set; }
      public string FullName { get; set; } = string.Empty;
      public string PhoneNumber { get; set; } = string.Empty;
      public string? Email { get; set; }
      public string? Notes { get; set; }
}

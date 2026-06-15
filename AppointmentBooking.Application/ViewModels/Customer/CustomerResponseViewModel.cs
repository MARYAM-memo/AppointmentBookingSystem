using AppointmentBooking.Application.ViewModels.Appointment;

namespace AppointmentBooking.Application.ViewModels.Customer;

public class CustomerResponseViewModel
{
      public CustomerViewModel Customer { get; set; } = null!;
      public List<AppointmentViewModel> Appointments { get; set; } = [];
      public decimal TotalSpent { get; set; }
}


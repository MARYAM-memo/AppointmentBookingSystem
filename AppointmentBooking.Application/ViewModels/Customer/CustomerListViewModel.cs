using AppointmentBooking.Application.ViewModels.General;

namespace AppointmentBooking.Application.ViewModels.Customer;

public class CustomerListViewModel : PaginationMetaData
{
      public List<CustomerViewModel> Customers { get; set; } = [];
      public string? SearchTerm { get; set; }
      public string? SortBy { get; set; }
}

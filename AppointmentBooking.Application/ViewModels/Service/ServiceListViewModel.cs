namespace AppointmentBooking.Application.ViewModels.Service;

public class ServiceListViewModel
{
      public IEnumerable<ServiceResponseViewModel>? Services { get; set; } = default;
      public List<string> Categories { get; set; } = [];
      public string? SearchTerm { get; set; }
      public string? SelectedCategory { get; set; }
      public bool ShowInactive { get; set; }
}

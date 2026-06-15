using AppointmentBooking.Application.ViewModels.General;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentBooking.Application.ViewModels.Appointment;

public class AppointmentListViewModel : PaginationMetaData
{
      public List<AppointmentViewModel> Appointments { get; set; } = [];
      public DateTime? SelectedDate { get; set; }
      public string? SelectedStatus { get; set; }
      public int? SelectedServiceId { get; set; }
      public int? SelectedCustomerId { get; set; }
      public string? SearchTerm { get; set; }
      public List<SelectListItem> StatusList { get; set; } = [];
}

using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentBooking.Application.DTOs;

public class FormDataCacheDTO
{
      public List<SelectListItem> ServicesList { get; set; } = [];
      public List<SelectListItem> CustomersList { get; set; } = [];
      public object? ServicesData { get; set; }
      public DateTime ExpiryTime { get; set; }
}

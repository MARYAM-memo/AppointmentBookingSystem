namespace AppointmentBooking.Application.DTOs;


public class AppointmentFiltersDTO : FilterDTO
{
    public DateTime? Date { get; set; }
    public string? Status { get; set; }
    public int? ServiceId { get; set; }
    public int? CustomerId { get; set; }
}

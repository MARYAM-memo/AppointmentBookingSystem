namespace AppointmentBooking.Core.Abstractions;

public interface ISoftDelete
{
      bool IsDeleted { get; set; }
      DateTime? DeletedAt { get; set; }
}

namespace AppointmentBooking.Core.Abstractions;

public interface IEntity
{
      int Id { get; set; }
      DateTime CreatedAt { get; set; }
      DateTime? UpdatedAt { get; set; }
}

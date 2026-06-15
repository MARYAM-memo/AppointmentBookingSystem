namespace AppointmentBooking.Application.ViewModels.General;

public enum MessageType { Success, Error, Warning, Info }

public class AlertMessage
{
      public string Title { get; set; } = null!;
      public MessageType Type { get; set; }
}

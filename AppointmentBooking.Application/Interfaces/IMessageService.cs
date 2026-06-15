namespace AppointmentBooking.Application.Interfaces;

public interface IMessageService
{
      /// <summary>
      /// Stores a success message in TempData for user feedback.
      /// </summary>
      void Success(string message);

      /// <summary>
      /// Stores an error message in TempData for user feedback.
      /// </summary>
      void Error(string message);

      /// <summary>
      /// Stores a warning message in TempData for user feedback.
      /// </summary>
      void Warning(string message);

      /// <summary>
      /// Stores an informational message in TempData for user feedback.
      /// </summary>
      void Info(string message);
}

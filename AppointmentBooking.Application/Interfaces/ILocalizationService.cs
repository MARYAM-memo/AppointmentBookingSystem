namespace AppointmentBooking.Application.Interfaces;

public interface ILocalizationService
{
      string this[string key] { get; }
      string GetString(string key);
      string GetString(string key, params object[] args);
}

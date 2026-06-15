namespace AppointmentBooking.Application.ViewModels.Account;

public class RegisterViewModel
{
      public required string Name { get; set; }
      public required string Email { get; set; }
      public required string Password { get; set; }
      public required string ConfirmPassword { get; set; }
}

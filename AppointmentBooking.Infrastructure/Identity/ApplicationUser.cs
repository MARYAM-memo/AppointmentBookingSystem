using AppointmentBooking.Application.Shared;
using AppointmentBooking.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace AppointmentBooking.Infrastructure.Identity;

public enum Gender { Male = 1, Female = 2 }

public class ApplicationUser : IdentityUser
{
  //Profile
  public string FullName { get; set; } = string.Empty;
  public DateTime? DateOfBirth { get; set; }
  public Gender? Gender { get; set; }

  //preferences
  public bool PreferDarkMode { get; set; } = false;
  public string PreferredLanguage { get; set; } = "ar";
  public bool EnableNotifications { get; set; } = true;

  //Audit & Status
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }
  public DateTime? LastLoginAt { get; set; }

  //Computed
  public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : UserName ?? Email ?? Constants.UserRole;
  public int? Age => DateOfBirth.HasValue
    ? DateTime.UtcNow.Year - DateOfBirth.Value.Year
    : null;
  public string JoinDate => CreatedAt.ToString("yyyy/MM/dd");

  //Navigation
  public ICollection<Appointment>? AppointmentsCreated { get; set; } = default;
  public ICollection<Customer>? CustomersCreated { get; set; } = default;
}

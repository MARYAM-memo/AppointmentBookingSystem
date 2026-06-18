using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace AppointmentBooking.Application.Validations.Account;

public class CustomPasswordValidator<TUser>(ILocalizationService localizer, IOptions<IdentitySettings> identitySettings) : IPasswordValidator<TUser> where TUser : class
{
      private readonly ILocalizationService _localizer=localizer;
      private readonly IOptions<IdentitySettings> _identitySettings=identitySettings;

      public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string? password)
      {
            if (string.IsNullOrEmpty(password))
            {
                  return IdentityResult.Failed(new IdentityError
                  {
                        Code = "PasswordRequired",
                        Description = _localizer["Password_Required"]
                  });
            }

            var errors = new List<IdentityError>();
            var settings = _identitySettings.Value.Password;

            // Check length
            if (password.Length < settings.RequiredLength)
            {
                  errors.Add(new IdentityError
                  {
                        Code = "PasswordTooShort",
                        Description = _localizer["Password_MinLength"]
                  });
            }

            if (password.Length > settings.RequiredMaxLength)
            {
                  errors.Add(new IdentityError
                  {
                        Code = "PasswordTooLong",
                        Description = _localizer["Password_MaxLength"]
                  });
            }

            // Check uppercase
            if (settings.RequireUppercase && !password.Any(char.IsUpper))
            {
                  errors.Add(new IdentityError
                  {
                        Code = "PasswordRequiresUpper",
                        Description = _localizer["Password_RequireUpper"]
                  });
            }

            // Check lowercase
            if (settings.RequireLowercase && !password.Any(char.IsLower))
            {
                  errors.Add(new IdentityError
                  {
                        Code = "PasswordRequiresLower",
                        Description = _localizer["Password_RequireLower"]
                  });
            }

            // Check digit
            if (settings.RequireDigit && !password.Any(char.IsDigit))
            {
                  errors.Add(new IdentityError
                  {
                        Code = "PasswordRequiresDigit",
                        Description = _localizer["Password_RequireDigit"]
                  });
            }

            // Check special character
            if (settings.RequireNonAlphanumeric)
            {
                  var specialChars = "!@#$%^&*(),.?\":{}|<>";
                  if (!password.Any(c => specialChars.Contains(c)))
                  {
                        errors.Add(new IdentityError
                        {
                              Code = "PasswordRequiresNonAlphanumeric",
                              Description = _localizer["Password_RequireSpecial"]
                        });
                  }
            }

            if (errors.Count != 0)
            {
                  return IdentityResult.Failed([.. errors]);
            }

            return IdentityResult.Success;
      }
}

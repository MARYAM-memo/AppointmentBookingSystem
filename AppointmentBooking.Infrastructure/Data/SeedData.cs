using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Core.Interfaces;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Infrastructure.Identity;
using AppointmentBooking.Application.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppointmentBooking.Infrastructure.Data;

public static class SeedData
{
      /// <summary>
      /// Initializes the application database asynchronously by setting up default business defaultProfile, seeding roleNames, and creating admin user.
      /// </summary>
      public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
      {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //Seed Business Profile
            await SeedBusinessProfileAsync(context, configuration);

            //Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Admin User
            await SeedAdminUserAsync(userManager, roleManager, configuration);
      }

      /// <summary>
      /// Seeds the database with a default business defaultProfile if none exists, using localized values for business name, custom labels, currency, and default settings (working hours 9 AM - 10 PM, 30-minute slots, Arabic language with RTL direction).
      /// </summary>
      static async Task SeedBusinessProfileAsync(DatabaseContext context, IConfiguration config)
      {
            if (context.BusinessProfiles.Any()) return;

            var businessSettingsSection = config.GetSection("BusinessSettings:DefaultProfile");

            var colorSection = businessSettingsSection.GetSection("Colors");
            var defaultProfile = new BusinessProfile
            {
                  BusinessName = businessSettingsSection["BusinessName"] ?? Constants.BusinessName,
                  BusinessType = businessSettingsSection["BusinessType"] ?? Constants.BusinessType,
                  CustomLabels = businessSettingsSection.GetSection("CustomLabels").Get<Dictionary<string, string>>() ?? new()
                  {
                      { "service", Constants.ServicesTab },
                      { "serviceItem", Constants.ServicesItem },
                      { "customer", Constants.CustomersTab },
                      { "appointment", Constants.AppointmentsTab },
                  },
                  Localization = new()
                  {
                        Currency = businessSettingsSection["Currency"] ?? Constants.DefaultCurrency,
                        Language = businessSettingsSection["Language"] ?? Constants.DefaultLanguage,
                        Direction = businessSettingsSection["Direction"] ?? Constants.DefaultDirection,
                        TimeZone = businessSettingsSection["TimeZone"] ?? Constants.DefaultTimeZone,
                  },
                  Colors = new()
                  {
                        Primary = colorSection["Primary"] ?? Constants.DefaultPrimaryColor,
                        Secondary = colorSection["Secondary"] ?? Constants.DefaultSecondaryColor,
                        Accent = colorSection["Accent"] ?? Constants.DefaultAccentColor,
                  },
                  WorkingHoursStart = TimeSpan.TryParse(businessSettingsSection["WorkingHoursStart"], out var workingHoursStart) ? workingHoursStart : TimeSpan.FromHours(Constants.Defaults.workingHoursStart),
                  WorkingHoursEnd = TimeSpan.TryParse(businessSettingsSection["WorkingHoursEnd"], out var workingHoursEnd) ? workingHoursEnd : TimeSpan.FromHours(Constants.Defaults.workingHoursEnd),
                  SlotDurationMinutes = int.TryParse(businessSettingsSection["SlotDurationMinutes"], out var slotDuration) ? slotDuration : Constants.Defaults.SlotDurationMinutes,
            };

            context.BusinessProfiles.Add(defaultProfile);
            await context.SaveChangesAsync();


      }

      /// <summary>
      /// Seeds default roleNames (Admin and User) into the identity system if they don't already exist.
      /// </summary>
      static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
      {
            string[] roleNames = [Constants.AdminRole, Constants.UserRole];
            foreach (var roleName in roleNames)
            {
                  var roleExist = await roleManager.RoleExistsAsync(roleName);
                  if (!roleExist)
                  {
                        var identityRole = new IdentityRole
                        {
                              Name = roleName,
                              NormalizedName = roleName.ToLower(),
                        };

                        await roleManager.CreateAsync(identityRole);
                  }
            }
      }

      /// <summary>
      /// Seeds the default admin user into the system if it doesn't exist, with predefined credentials and roleName assignment.
      /// </summary>
      static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
      {

            var adminEmail = configuration["AdminSettings:Email"] ?? Constants.AdminEmail;
            var adminPassword = configuration["AdminSettings:Password"] ?? "";
            var adminPhone = configuration["AdminSettings:Phone"] ?? Constants.AdminPhone;
            var adminRole = configuration["AdminSettings:Role"] ?? Constants.AdminRole;

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                  adminUser = new ApplicationUser
                  {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = adminRole,
                        EmailConfirmed = true,
                        PhoneNumber = adminPhone,
                        DateOfBirth = new DateTime(1999, 12, 2),
                        Gender = Gender.Male,
                  };

                  var result = await userManager.CreateAsync(adminUser, adminPassword);

                  if (result.Succeeded)
                  {
                        if (await roleManager.RoleExistsAsync(adminRole))
                              await userManager.AddToRoleAsync(adminUser, adminRole);
                        await userManager.UpdateAsync(adminUser);
                  }
                  else
                  {
                        var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create admin user: {errorMessages}");
                  }
            }
      }
}

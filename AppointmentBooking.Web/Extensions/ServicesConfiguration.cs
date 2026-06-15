using System.Globalization;
using System.Threading.RateLimiting;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.Settings;
using AppointmentBooking.Application.Validations.Account;
using AppointmentBooking.Application.Validations.Appointment;
using AppointmentBooking.Application.Validations.BusinessProfile;
using AppointmentBooking.Application.Validations.Customer;
using AppointmentBooking.Application.Validations.Service;
using AppointmentBooking.Core.Interfaces;
using AppointmentBooking.Infrastructure.Data;
using AppointmentBooking.Infrastructure.DataAccess;
using AppointmentBooking.Infrastructure.Identity;
using AppointmentBooking.Infrastructure.Mapping;
using AppointmentBooking.Infrastructure.Services;
using AppointmentBooking.Application.Shared;
using AppointmentBooking.Web.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AppointmentBooking.Web.Extensions
{

      public static class DatabaseExtensions
      {
            /// <summary>
            /// Registers database services including DbContext with Npgsql and UnitOfWork with scoped lifetime.
            /// </summary>
            public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
            {
                  var connectionString = configuration.GetConnectionString("DefaultConnection");
                  services.AddDbContext<DatabaseContext>(options =>
                      options.UseNpgsql(connectionString));

                  services.AddScoped<IUnitOfWork, UnitOfWork>();
                  return services;
            }
      }

      public static class IdentityExtensions
      {
            /// <summary>
            /// Configures Identity services with ApplicationUser, IdentityRole, EntityFramework storage, and cookie authentication settings.
            /// </summary>
            public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
            {
                  services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                  {
                        configuration.GetSection("Identity").Bind(options);
                  })
                  .AddEntityFrameworkStores<DatabaseContext>()
                  .AddDefaultTokenProviders();

                  services.ConfigureApplicationCookie(options =>
                  {
                        options.Cookie.HttpOnly = true;
                        options.ExpireTimeSpan = TimeSpan.FromDays(7);
                        options.LoginPath = "/Account/Login";
                        options.LogoutPath = "/Account/Logout";
                        options.SlidingExpiration = true;
                  });

                  return services;
            }
      }

      public static class ValidationExtensions
      {
            /// <summary>
            /// Registers FluentValidation with auto-validation (disables DataAnnotations), client-side adapters, and validators from multiple assemblies.
            /// </summary>
            public static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
            {
                  /* services.AddFluentValidationAutoValidation(config =>
                  {
                        config.DisableDataAnnotationsValidation = true;
                  }); */
                  services.AddFluentValidationClientsideAdapters();

                  services.AddValidatorsFromAssemblyContaining<LoginValidation>();
                  services.AddValidatorsFromAssemblyContaining<BrandingColorsValidator>();
                  services.AddValidatorsFromAssemblyContaining<ServiceRequestValidation>();
                  services.AddValidatorsFromAssemblyContaining<AppointmentRequestValidator>();
                  services.AddValidatorsFromAssemblyContaining<CustomerRequestValidator>();

                  return services;
            }
      }

      public static class ApplicationServicesExtensions
      {
            /// <summary>
            /// Registers all application services including Mapster mappings, business services (profile, theme, message, dashboard, availability, appointment), rate limiting policies,localization, and HTTP context accessor.
            /// </summary>
            public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
            {
                  // Register Mapster
                  MapsterConfig.RegisterMappings();

                  // Dependency Injection
                  services.AddScoped<IBusinessProfileService, BusinessProfileService>();
                  services.AddScoped<IMessageService, MessageService>();
                  services.AddScoped<IDashboardRepository, DashboardRepository>();
                  services.AddScoped<IDashboardService, DashboardService>();
                  services.AddScoped<IAvailabilityService, AvailabilityService>();
                  services.AddScoped<IFormPreparationService, FormPreparationService>();
                  services.AddScoped<IAppointmentViewModelService, AppointmentViewModelService>();
                  services.AddScoped<IAppointmentService, AppointmentService>();
                  services.AddScoped<ILocalizationService, ValidationLocalizer>();

                  //IMapper from Mapster
                  services.AddSingleton(TypeAdapterConfig.GlobalSettings);
                  services.AddScoped<IMapper, ServiceMapper>();

                  // Register Settings
                  services.Configure<LocalizationSettings>(configuration.GetSection("LocalizationSettings"));
                  services.Configure<RateLimitingSettings>(configuration.GetSection("RateLimiting"));

                  services.AddHttpContextAccessor();

                  // Localization services
                  services.AddApplicationLocalization();

                  var limit = configuration.GetSection("RateLimiting:Policies").Get<Dictionary<string, RateLimitPolicy>>()
                          ?? [];

                  services.AddRateLimiter(options =>
                  {
                        var policies = new Dictionary<string, (int permitLimit, int windowMinutes, int queueLimit)>
                        {
                              ["strict"] = (10, 1, 2),
                              ["default"] = (50, 1, 5),
                              ["light"] = (100, 1, 10),
                              ["auth"] = (5, 1, 1)
                        };

                        foreach (var policy in policies)
                        {
                              var policyName = policy.Key;
                              var (permitLimit, windowMinutes, queueLimit) = policy.Value;

                              // Get settings from the configuration if available
                              if (limit.TryGetValue(policyName, out var customPolicy))
                              {
                                    permitLimit = customPolicy.PermitLimit;
                                    windowMinutes = customPolicy.WindowMinutes;
                                    queueLimit = customPolicy.QueueLimit;
                              }

                              options.AddFixedWindowLimiter(policyName, opt =>
                              {
                                    opt.PermitLimit = permitLimit;
                                    opt.Window = TimeSpan.FromMinutes(windowMinutes);
                                    opt.QueueLimit = queueLimit;
                                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                              });
                        }

                        options.OnRejected = async (context, cancellationToken) =>
                        {
                              context.HttpContext.Response.StatusCode = 429;
                              context.HttpContext.Response.Headers.RetryAfter = "60";

                              context.HttpContext.Items["OriginalPath"] = context.HttpContext.Request.Path;
                              context.HttpContext.Response.Redirect($"/error/429?path={Uri.EscapeDataString(context.HttpContext.Request.Path)}");
                        };

                  });

                  return services;
            }

            /// <summary>
            /// Configures application localization with Arabic (Egypt) as default culture, support for multiple cultures from configuration, and enables reading culture from URL query string (e.g., ?culture=en-US).
            /// </summary>
            public static IServiceCollection AddApplicationLocalization(this IServiceCollection services)
            {
                  services.AddLocalization(options => options.ResourcesPath = string.Empty);

                  services.Configure<RequestLocalizationOptions>(options =>
                  {
                        options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
                        options.RequestCultureProviders.Insert(1, new CookieRequestCultureProvider());
                  });

                  // Post-configure to use the registered settings
                  services.AddOptions<RequestLocalizationOptions>()
                      .PostConfigure<IOptions<LocalizationSettings>>((options, settings) =>
                      {
                            var config = settings.Value;
                            var defaultCulture = config.DefaultCulture;

                            var supportedCultures = new List<CultureInfo>();

                            foreach (var lang in config.AvailableLanguages)
                            {
                                  if (config.SupportedCultures != null && config.SupportedCultures.TryGetValue(lang, out var cultureName))
                                  {
                                        supportedCultures.Add(new CultureInfo(cultureName));
                                  }
                                  else
                                  {
                                        var fallbackCulture = Methods.CultureSwitch(lang);
                                        supportedCultures.Add(new CultureInfo(fallbackCulture));
                                  }
                            }

                            if (supportedCultures.Count == 0)
                            {
                                  supportedCultures.Add(new CultureInfo("ar-EG"));
                                  supportedCultures.Add(new CultureInfo("en-US"));
                            }

                            options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                            options.SupportedCultures = supportedCultures;
                            options.SupportedUICultures = supportedCultures;
                      });

                  return services;
            }
      }
}

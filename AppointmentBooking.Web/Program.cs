using AppointmentBooking.Web.Extensions;
using AppointmentBooking.Web.Middleware;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.Razor;
using Serilog;

// Read the config first
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();
    
    // ================Basic Services===
    builder.Host.UseSerilog();
    builder.Services.AddDatabaseServices(builder.Configuration);
    builder.Services.AddIdentityServices(builder.Configuration);
    builder.Services.AddFluentValidationServices();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization()
    .AddSessionStateTempDataProvider();

    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    // ================Improve performance================
    builder.Services.AddResponseCaching();
    builder.Services.AddMemoryCache();
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    // ================Database Seeding=
    await app.InitializeDatabaseAsync(builder.Configuration);

    // ================Error Handling===
    app.UseCustomExceptionHandler();

    //===============STATIC FILES - CRITICAL for CSS/JS=======
    app.UseStaticFiles();

    // ================Security Headers=================
    app.UseSecurityHeaders();

    // =================Routing & Auth=========
    app.UseSession();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // ================Status Code=================
    app.UseCustomStatusCodePages();

    // =================Localization====
    app.UseMiddleware<LocalizationMiddleware>();

    // =================Rate Limiter=========
    app.UseRateLimiter();

    // =================Routing=========
    app.MapErrorRoutes();
    app.MapDefaultRoutes();

    // =================Run==========
    app.Run();
}
catch (HostAbortedException)
{
    // Ignore EF Design-Time host shutdown
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();// Ensure all records are written to the file before closing the application
}

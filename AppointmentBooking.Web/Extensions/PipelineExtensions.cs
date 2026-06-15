using AppointmentBooking.Application.Extensions;
using AppointmentBooking.Infrastructure.Data;
using AppointmentBooking.Application.Shared;

namespace AppointmentBooking.Web.Extensions;

public static class PipelineExtensions
{
      /// <summary>
      /// Initial data setup at startup
      /// </summary>
      public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app, IConfiguration configuration)
      {
            using var scope = app.Services.CreateScope();
            await SeedData.InitializeAsync(scope.ServiceProvider, configuration);

            GetCurrency(scope);
            return app;
      }

      /// <summary>
      /// Adds security headers to protect the application from XSS, Clickjacking, and sniffing attacks.
      /// </summary>
      public static WebApplication UseSecurityHeaders(this WebApplication app)
      {
            app.Use(async (context, next) =>
            {
                  context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                  context.Response.Headers.Append("X-Frame-Options", "DENY");
                  context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

                  var csp = "default-src 'self'; " +
                            "script-src 'self' https://code.jquery.com https://cdnjs.cloudflare.com https://cdn.jsdelivr.net 'unsafe-inline' 'unsafe-eval'; " +
                            "style-src 'self' https://cdn.jsdelivr.net https://fonts.googleapis.com 'unsafe-inline'; " +
                            "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net data:; " +
                            "connect-src 'self' ws://localhost:* wss://localhost:* https:; " +
                            "img-src 'self' data: https:;";

                  context.Response.Headers.Append("Content-Security-Policy", csp);

                  await next();
            });

            return app;
      }

      /// <summary>
      /// Configuring the error handler according to the operating environment
      /// </summary>
      public static WebApplication UseCustomExceptionHandler(this WebApplication app)
      {
            if (app.Environment.IsDevelopment())
            {
                  app.UseDeveloperExceptionPage();
                  app.UseHttpsRedirection();
            }
            else
            {
                  app.UseExceptionHandler("/error/server-error");
                  app.UseHsts();
                  app.UseHttpsRedirection();
            }

            return app;
      }

      /// <summary>
      /// Configuring the Status Codes handler centrally
      /// </summary>
      public static WebApplication UseCustomStatusCodePages(this WebApplication app)
      {
            app.UseStatusCodePages(async context =>
            {
                  var response = context.HttpContext.Response;
                  var request = context.HttpContext.Request;

                  // Avoid redirection in special cases
                  if (ShouldSkipRedirect(request))
                        return;

                  // Save the original path
                  context.HttpContext.Items[Constants.Keys.ItemsOriginalPath] = request.Path;

                  var redirectUrl = GetRedirectUrl(response.StatusCode, request.Path);
                  if (!string.IsNullOrEmpty(redirectUrl))
                  {
                        response.Redirect(redirectUrl);
                  }
            });

            return app;
      }

      /// <summary>
      /// Configuring all Error Handling paths
      /// </summary>
      public static WebApplication MapErrorRoutes(this WebApplication app)
      {
            app.MapControllerRoute(
                name: "error",
                pattern: "error/{code:int}",
                defaults: new { controller = "Errors", action = "Handle" });

            app.MapControllerRoute(
                name: "error-access-denied",
                pattern: "error/access-denied",
                defaults: new { controller = "Errors", action = "AccessDenied" });

            app.MapControllerRoute(
                name: "error-not-found",
                pattern: "error/not-found",
                defaults: new { controller = "Errors", action = "NotFoundPage" });

            app.MapControllerRoute(
                name: "error-server",
                pattern: "error/server-error",
                defaults: new { controller = "Errors", action = "ServerError" });

            return app;
      }

      /// <summary>
      /// Configuring the main application path
      /// </summary>
      public static WebApplication MapDefaultRoutes(this WebApplication app)
      {
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dashboard}/{action=Index}/{id?}")
                .WithStaticAssets();

            return app;
      }

      //===================Helper Methods=================

      /// <summary>
      /// Determines whether the redirect middleware should skip processing for certain paths (error, API, AJAX requests).
      /// </summary>
      static bool ShouldSkipRedirect(HttpRequest request)
      {
            return request.Path.StartsWithSegments("/error") ||
                   request.Path.StartsWithSegments("/api") ||
                   request.Headers.XRequestedWith == "XMLHttpRequest";
      }

      /// <summary>
      /// Maps HTTP status codes to appropriate error page redirect URLs.
      /// </summary>
      static string GetRedirectUrl(int statusCode, string originalPath)
      {
            return statusCode switch
            {
                  401 or 403 => "/error/access-denied",
                  404 => $"/error/not-found?path={originalPath}",
                  405 or 408 or 409 or 422 or 429 => $"/error/{statusCode}",
                  500 or 502 or 503 or 504 => "/error/server-error",
                  _ when statusCode >= 400 => $"/error/{statusCode}",
                  _ => null
            } ?? "";
      }

      /// <summary>
      /// Retrieves the current currency from the business profile and sets it on the PriceExtension class.
      /// </summary>
      static void GetCurrency(IServiceScope scope)
      {
            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var profile = dbContext.BusinessProfiles.FirstOrDefault();
            PriceExtension.CurrentCurrency = profile?.Localization?.Currency ?? Constants.DefaultCurrency;
      }
}


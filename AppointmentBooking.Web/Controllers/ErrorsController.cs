using AppointmentBooking.Application.ViewModels.Account;
using AppointmentBooking.Application.Shared;
using AppointmentBooking.Web.Resources.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AppointmentBooking.Web.Controllers;

/// <summary>
/// Centralized Error Controller - Automatically displays error pages
/// based on HTTP status code without requiring manual invocation
/// </summary>
[AllowAnonymous]
public class ErrorsController(ILogger<ErrorsController> logger, IWebHostEnvironment environment, IStringLocalizer<MessagesSharedResource> localizer) : Controller
{
      readonly ILogger<ErrorsController> _logger = logger;
      readonly IWebHostEnvironment _environment = environment;
      readonly IStringLocalizer<MessagesSharedResource> _localizer = localizer;

      /// <summary>
      /// Main error handler - automatically identifies the page based on the code
      /// Route: /error/{code}
      /// </summary>
      [Route("error/{statusCode:int}")]
      public IActionResult Handle(int statusCode)
      {
            var traceId = HttpContext.TraceIdentifier;
            var originalPath = HttpContext.Items[Constants.Keys.ItemsOriginalPath]?.ToString()
                ?? Request.Query["path"].ToString()
                ?? _localizer["Error_UnknownPath"];

            _logger.LogInformation(
                "Rendering error page for status code: {StatusCode} | Path: {Path} | TraceId: {TraceId}",
                statusCode, originalPath, traceId);

            var errorData = new ErrorPageViewModel
            {
                  StatusCode = statusCode,
                  Title = GetErrorTitle(statusCode),
                  Message = GetErrorMessage(statusCode),
                  Description = GetErrorDescription(statusCode),
                  Icon = GetErrorIcon(statusCode),
                  IconColor = GetErrorIconColor(statusCode),
                  TraceId = traceId,
                  RequestPath = originalPath,
                  IsDevelopment = _environment.IsDevelopment(),
                  ShowDetails = _environment.IsDevelopment(),
                  Suggestions = GetSuggestions(statusCode)
            };

            if (_environment.IsDevelopment())
            {
                  var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerFeature>();
                  if (exceptionHandler?.Error != null)
                  {
                        errorData.ExceptionDetails = $"{exceptionHandler.Error.Message}\n\n{exceptionHandler.Error.StackTrace}";
                  }
            }

            var viewName = GetErrorViewName(statusCode);

            Response.StatusCode = statusCode;
            return View(viewName, errorData);
      }

      /// <summary>
      /// 403 Denied Access Page - Displayed automatically from UseStatusCodePages
      /// </summary>
      [Route("error/access-denied")]
      public IActionResult AccessDenied()
      {
            return Handle(403);
      }

      /// <summary>
      /// Page not found (404) - Displayed automatically from UseStatusCodePages
      /// </summary>
      [Route("error/not-found")]
      public IActionResult NotFoundPage()
      {
            return Handle(404);
      }

      /// <summary>
      /// Server Error Page (500) - Displayed automatically from ExceptionHandler
      /// </summary>
      [Route("error/server-error")]
      public IActionResult ServerError()
      {
            return Handle(500);
      }


      /// <summary>
      /// Page of many orders (429) - Displayed automatically by RateLimiter
      /// </summary>
      [Route("error/too-many-requests")]
      public IActionResult TooManyRequests()
      {
            return Handle(429);
      }

      /// <summary>
      /// General error page - for manual use if necessary
      /// </summary>
      [Route("error/general")]
      public IActionResult GeneralError(string message, string? title = null, int? statusCode = null)
      {
            var errorData = new ErrorPageViewModel
            {
                  StatusCode = statusCode ?? 400,
                  Title = title ?? _localizer["Error_GeneralTitle"],
                  Message = message ?? _localizer["Error_GeneralMessage"],
                  Icon = "bi-exclamation-triangle",
                  IconColor = "#f59e0b",
                  TraceId = HttpContext.TraceIdentifier
            };

            return View("ErrorGeneral", errorData);
      }

      // ============================================
      // Helpers - Defining the View and Content
      // ============================================

      /// <summary>
      /// Maps an HTTP status code to the corresponding error view name.
      /// </summary>
      private static string GetErrorViewName(int statusCode) => statusCode switch
      {
            400 => "Error400",           // Bad Request
            401 => "Error401",           // Unauthorized
            403 => "Error403",           // Forbidden
            404 => "Error404",           // Not Found
            405 => "Error405",           // Method Not Allowed
            408 => "Error408",           // Request Timeout
            409 => "Error409",           // Conflict
            422 => "Error422",           // Unprocessable Entity
            429 => "Error429",           // Too Many Requests
            500 => "Error500",           // Internal Server Error
            502 => "Error502",           // Bad Gateway
            503 => "Error503",           // Service Unavailable
            504 => "Error504",           // Gateway Timeout
            _ => "ErrorGeneric"          // Any other statusCode
      };

      /// <summary>
      /// Returns the localized error title for a given HTTP status code.
      /// </summary>
      private string GetErrorTitle(int statusCode) => statusCode switch
      {
            400 => _localizer["Error_Title_400"],
            401 => _localizer["Error_Title_401"],
            403 => _localizer["Error_Title_403"],
            404 => _localizer["Error_Title_404"],
            405 => _localizer["Error_Title_405"],
            408 => _localizer["Error_Title_408"],
            409 => _localizer["Error_Title_409"],
            422 => _localizer["Error_Title_422"],
            429 => _localizer["Error_Title_429"],
            500 => _localizer["Error_Title_500"],
            502 => _localizer["Error_Title_502"],
            503 => _localizer["Error_Title_503"],
            504 => _localizer["Error_Title_504"],
            _ => _localizer["Error_Title_Default"]
      };

      /// <summary>
      /// Returns the localized error message description for a given HTTP status code.
      /// </summary>
      private string GetErrorMessage(int statusCode) => statusCode switch
      {
            400 => _localizer["Error_Message_400"],
            401 => _localizer["Error_Message_401"],
            403 => _localizer["Error_Message_403"],
            404 => _localizer["Error_Message_404"],
            405 => _localizer["Error_Message_405"],
            408 => _localizer["Error_Message_408"],
            409 => _localizer["Error_Message_409"],
            422 => _localizer["Error_Message_422"],
            429 => _localizer["Error_Message_429"],
            500 => _localizer["Error_Message_500"],
            502 => _localizer["Error_Message_502"],
            503 => _localizer["Error_Message_503"],
            504 => _localizer["Error_Message_504"],
            _ => _localizer["Error_Message_Default"]
      };

      /// <summary>
      /// Returns the localized error resolution instructions for a given HTTP status code.
      /// </summary>
      private string GetErrorDescription(int statusCode) => statusCode switch
      {
            400 => _localizer["Error_Description_400"],
            401 => _localizer["Error_Description_401"],
            403 => _localizer["Error_Description_403"],
            404 => _localizer["Error_Description_404"],
            405 => _localizer["Error_Description_405"],
            408 => _localizer["Error_Description_408"],
            409 => _localizer["Error_Description_409"],
            422 => _localizer["Error_Description_422"],
            429 => _localizer["Error_Description_429"],
            500 => _localizer["Error_Description_500"],
            502 => _localizer["Error_Description_502"],
            503 => _localizer["Error_Description_503"],
            504 => _localizer["Error_Description_504"],
            _ => ""
      };

      /// <summary>
      /// Returns the Bootstrap icon class name representing the error type for a given HTTP status code.
      /// </summary>
      private static string GetErrorIcon(int statusCode) => statusCode switch
      {
            400 => "bi-exclamation-circle",
            401 => "bi-shield-lock",
            403 => "bi-shield-exclamation",
            404 => "bi-search",
            405 => "bi-arrow-left-right",
            408 => "bi-clock-history",
            409 => "bi-arrow-repeat",
            422 => "bi-x-octagon",
            429 => "bi-speedometer",
            500 => "bi-bug",
            502 => "bi-hdd-network",
            503 => "bi-cloud-slash",
            504 => "bi-wifi-off",
            _ => "bi-exclamation-triangle"
      };

      /// <summary>
      /// Returns the hex color code for the error icon based on the HTTP status code.
      /// </summary>
      private static string GetErrorIconColor(int statusCode) => statusCode switch
      {
            400 => "#f59e0b",   // Amber
            401 => "#dc2626",   // Red
            403 => "#dc2626",   // Red
            404 => "#4f46e5",   // Indigo
            405 => "#f59e0b",   // Amber
            408 => "#f59e0b",   // Amber
            409 => "#f59e0b",   // Amber
            422 => "#db2777",   // Pink
            429 => "#f59e0b",   // Amber
            500 => "#dc2626",   // Red
            502 => "#dc2626",   // Red
            503 => "#4f46e5",   // Indigo
            504 => "#dc2626",   // Red
            _ => "#6b7280"      // Gray
      };

      /// <summary>
      /// Returns a list of contextual suggestions (actions with icons and URLs) to help users recover from the error.
      /// </summary>
      private List<ErrorSuggestion> GetSuggestions(int statusCode) => statusCode switch
      {
            401 =>
            [
                new() { Text = _localizer["Error_Suggestion_Login"], Url = Url.Action("Login", "Account")??"", Icon = "bi-box-arrow-in-right" },
                new() { Text = _localizer["Error_Suggestion_Register"], Url = Url.Action("Register", "Account")??"", Icon = "bi-person-plus" }
            ],
            403 =>
            [
                new() { Text = _localizer["Error_Suggestion_HomePage"], Url = "/", Icon = "bi-house" },
                new() { Text = _localizer["Error_Suggestion_ContactSupport"], Url = "#", Icon = "bi-headset" }
            ],
            404 =>
            [
                new() { Text = _localizer["Error_Suggestion_HomePage"], Url = "/", Icon = "bi-house" },
                new() { Text = _localizer["Error_Suggestion_Services"], Url = Url.Action("Index", "Services")??"", Icon = "bi-grid" },
                new() { Text = _localizer["Error_Suggestion_Appointments"], Url = Url.Action("Index", "Appointments")??"", Icon = "bi-calendar" }
            ],
            429 =>
            [
                new() { Text = _localizer["Error_Suggestion_Retry"], Url = "javascript:location.reload()", Icon = "bi-arrow-clockwise" },
                new() { Text = _localizer["Error_Suggestion_HomePage"], Url = "/", Icon = "bi-house" },
                new() { Text = _localizer["Error_Suggestion_MyAppointments"], Url = Url.Action("Index", "Appointments") ?? "/", Icon = "bi-calendar" }
            ],
            500 =>
            [
                new() { Text = _localizer["Error_Suggestion_Retry"], Url = "javascript:location.reload()", Icon = "bi-arrow-clockwise" },
                new() { Text = _localizer["Error_Suggestion_HomePage"], Url = "/", Icon = "bi-house" }
            ],
            _ =>
            [
                new() { Text = _localizer["Error_Suggestion_HomePage"], Url = "/", Icon = "bi-house" },
                new() { Text = _localizer["Error_Suggestion_Back"], Url = "javascript:history.back()", Icon = "bi-arrow-right" }
            ]
      };
}


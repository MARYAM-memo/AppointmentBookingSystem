using AppointmentBooking.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace AppointmentBooking.Application.Extensions;

public static class BookingStatusExtensions
{
      static IStringLocalizer? _localizer;

      /// <summary>
      /// Initialize the localizer from the MVC projec after building the app.
      /// </summary>
      public static void InitializeLocalizer(IStringLocalizer localizer)
      {
            _localizer = localizer;
      }

      /// <summary>
      /// Returns the localized display name for a appointment status. Falls back to Arabic hardcoded values if localization service is unavailable.
      /// </summary>
      public static string GetLocalizedName(this BookingStatus status)
      {
            if (_localizer != null)
            {
                  var key = $"BookingStatus_{status}";
                  var localizedValue = _localizer[key];

                  // Check if the key exists (not returning the key itself)
                  if (localizedValue != key)
                        return localizedValue;
            }

            return status switch
            {
                  BookingStatus.Pending => "معلق",
                  BookingStatus.Confirmed => "مؤكد",
                  BookingStatus.InProgress => "جاري",
                  BookingStatus.Completed => "مكتمل",
                  BookingStatus.Cancelled => "ملغي",
                  BookingStatus.NoShow => "لم يحضر",
                  BookingStatus.Rescheduled => "معاد جدولة",
                  _ => status.ToString()
            };
      }

      /// <summary>
      /// Returns the localized action button text for a appointment status (e.g., "Confirm", "Start", "Complete", "Cancel") with fallback to Arabic defaults.
      /// </summary>
      public static string GetActionText(this BookingStatus status)
      {
            if (_localizer != null)
            {
                  var key = status switch
                  {
                        BookingStatus.Confirmed => "Action_Confirm",
                        BookingStatus.InProgress => "Action_Start",
                        BookingStatus.Completed => "Action_Complete",
                        BookingStatus.Cancelled => "Action_Cancel",
                        _ => "Action_Update"
                  };

                  var localizedValue = _localizer[key];
                  if (localizedValue != key)
                        return localizedValue;
            }

            return status switch
            {
                  BookingStatus.Confirmed => "تأكيد",
                  BookingStatus.InProgress => "بدء",
                  BookingStatus.Completed => "إكمال",
                  BookingStatus.Cancelled => "إلغاء",
                  _ => "تحديث"
            };
      }

      /// <summary>
      /// Returns the Bootstrap/utility CSS class name for styling the status badge (e.g., warning, info, success, danger).
      /// </summary>
      public static string GetCssClass(this BookingStatus status)
      {
            return status switch
            {
                  BookingStatus.Pending => "warning",
                  BookingStatus.Confirmed => "info",
                  BookingStatus.InProgress => "primary",
                  BookingStatus.Completed => "success",
                  BookingStatus.Cancelled => "danger",
                  BookingStatus.NoShow => "secondary",
                  BookingStatus.Rescheduled => "dark",
                  _ => "secondary"
            };
      }

      /// <summary>
      /// Returns the hex color code associated with the appointment status for UI consistency.
      /// </summary>
      public static string GetCssColor(this BookingStatus status)
      {
            return status switch
            {
                  BookingStatus.Confirmed => "#0d6efd",
                  BookingStatus.InProgress => "#6f42c1",
                  BookingStatus.Completed => "#198754",
                  BookingStatus.Cancelled => "#dc3545",
                  BookingStatus.Pending => "#00e1ff",
                  BookingStatus.NoShow => "#7b9b9f",
                  BookingStatus.Rescheduled => "#924357",
                  _ => "#6c757d"
            };
      }

      /// <summary>
      /// Returns the hex color code associated with the appointment status for UI consistency.
      /// </summary>
      public static string GetCssColor(this string? statusString)
      {
            if (string.IsNullOrWhiteSpace(statusString))
                  return "#6c757d";

            if (Enum.TryParse<BookingStatus>(statusString, true, out var status))
                  return status.GetCssColor();

            return statusString.ToLowerInvariant() switch
            {
                  "confirmed" => "#0d6efd",
                  "inprogress" => "#6f42c1",
                  "in-progress" => "#6f42c1",
                  "completed" => "#198754",
                  "cancelled" => "#dc3545",
                  "canceled" => "#dc3545",
                  "pending" => "#00e1ff",
                  "noshow" => "#7b9b9f",
                  "no-show" => "#7b9b9f",
                  "rescheduled" => "#924357",
                  _ => "#6c757d"
            };
      }

      /// <summary>
      /// Returns the Bootstrap icon name representing the appointment status (e.g., check-circle, x-circle, play-circle).
      /// </summary>
      public static string GetCssIcon(this BookingStatus status)
      {
            return status switch
            {
                  BookingStatus.Pending => "hourglass-split",
                  BookingStatus.Confirmed => "check-circle",
                  BookingStatus.InProgress => "play-circle",
                  BookingStatus.Completed => "trophy",
                  BookingStatus.Cancelled => "x-circle",
                  BookingStatus.NoShow => "person-x",
                  BookingStatus.Rescheduled => "arrow-repeat",
                  _ => "circle"
            };
      }

      /// <summary>
      /// Returns a list of all appointment statuses as SelectListItem with localized names, suitable for dropdown menus.
      /// </summary>
      public static List<SelectListItem> GetAllStatusesWithLocalizedName()
      {
            return [.. Enum.GetValues<BookingStatus>()
                .Select(status => new SelectListItem
                {
                      Value = status.ToString(),
                      Text = status.GetLocalizedName()
                })];
      }
}

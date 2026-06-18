namespace AppointmentBooking.Application.Shared;

public static class Constants
{
      public static string AdminEmail => "admin@system.com";
      public static string AdminPhone => "01234567890";
      public const string AdminRole = "Admin";
      public static string UserRole => "User";

      public static string BusinessName => "نظام حجز المواعيد";
      public static string BusinessType => "شامل";
      public static string ServicesTab => "الخدمات";
      public static string ServicesItem => "الخدمة";
      public static string CustomersTab => "العملاء";
      public static string AppointmentsTab => "الحجوزات";
      public static string DefaultCurrency => "ج.م";
      public static string DefaultLanguage => "ar";
      public static string DefaultCulture => "ar-EG";
      public static string DefaultDirection => "rtl";
      public static string DefaultPrimaryColor => "#2c6e7c";
      public static string DefaultSecondaryColor => "#3a9e8f";
      public static string DefaultAccentColor => "#f39c12";

      public static class Defaults
      {
            public const int SlotDurationMinutes = 30;
            public const int workingHoursStart = 9;
            public const int workingHoursEnd = 22;
            public const int MinLogicalWorkingHours = 4;
            public const int DefaultPageSize = 25;
            public const int LoadPageSize = 50;
            public const string ServiceIcon = "bi-stars";
      }

      public static class Keys
      {
            public static string CookieUserLanguage => "UserLanguage";
            public static string ItemsCurrentProfile => "CurrentBusinessProfile";
            public static string ItemsOriginalPath => "OriginalPath";
            public static string ItemsHtmlLang => "HtmlLang";
            public static string CacheFormSelectList => "Form_Select_Lists";
      }
}

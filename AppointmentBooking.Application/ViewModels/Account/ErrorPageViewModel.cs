namespace AppointmentBooking.Application.ViewModels.Account;

public class ErrorPageViewModel
{
      public int StatusCode { get; set; }
      public string Title { get; set; } = string.Empty;
      public string Message { get; set; } = string.Empty;
      public string Description { get; set; } = string.Empty;
      public string Icon { get; set; } = "bi-exclamation-triangle";
      public string IconColor { get; set; } = "#6b7280";
      public string? TraceId { get; set; }
      public string? RequestPath { get; set; }
      public bool IsDevelopment { get; set; }
      public bool ShowDetails { get; set; }
      public string? ExceptionDetails { get; set; }
      public List<ErrorSuggestion> Suggestions { get; set; } = [];
}

public class ErrorSuggestion
{
      public string Text { get; set; } = string.Empty;
      public string Url { get; set; } = string.Empty;
      public string Icon { get; set; } = "bi-arrow-right";
}
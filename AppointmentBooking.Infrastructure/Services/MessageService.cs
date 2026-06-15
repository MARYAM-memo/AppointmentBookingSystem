using System.Text.Json;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.General;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AppointmentBooking.Infrastructure.Services;

public class MessageService(ITempDataDictionaryFactory tempDataFactory, IHttpContextAccessor httpContextAccessor) : IMessageService
{
      readonly ITempDataDictionaryFactory _tempDataFactory = tempDataFactory;
      readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

      /// <summary>
      /// Serializes and stores an alert message in TempData for the next request.
      /// </summary>
      void SetMessage(string text, MessageType type)
      {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
                  return;

            var tempData = _tempDataFactory.GetTempData(httpContext);

            var message = new AlertMessage { Title = text, Type = type };
            var json = JsonSerializer.Serialize(message);

            tempData["Message"] = json;

            tempData.Peek("Message");
      }

      public void Success(string message)
        => SetMessage(message, MessageType.Success);

      public void Error(string message)
          => SetMessage(message, MessageType.Error);

      public void Warning(string message)
          => SetMessage(message, MessageType.Warning);

      public void Info(string message)
          => SetMessage(message, MessageType.Info);
}

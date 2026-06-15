using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Core.Interfaces;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Application.Shared;
using AppointmentBooking.Web.Resources.Messages;
using FluentValidation;
using FluentValidation.AspNetCore;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AppointmentBooking.Web.Controllers;

public abstract class BaseController : Controller
{
      private IBusinessProfileService? _profileService;
      private IUnitOfWork? _unitOfWork;
      private IMapper? _mapper;
      private IMessageService? _messageService;
      private IStringLocalizer<MessagesSharedResource>? _localizer;

      // Protected properties for lazy loading
      protected IBusinessProfileService ProfileService =>
          _profileService ??= HttpContext.RequestServices.GetRequiredService<IBusinessProfileService>();

      protected IUnitOfWork UnitOfWork =>
          _unitOfWork ??= HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();

      protected IMapper Mapper =>
          _mapper ??= HttpContext.RequestServices.GetRequiredService<IMapper>();

      protected IMessageService MessageService =>
          _messageService ??= HttpContext.RequestServices.GetRequiredService<IMessageService>();
      protected IStringLocalizer<MessagesSharedResource> Localizer =>
            _localizer ??= HttpContext.RequestServices.GetRequiredService<IStringLocalizer<MessagesSharedResource>>();

      /// <summary>
      /// Get the Business Profile with Caching at the single Request level
      /// </summary>
      protected async Task<BusinessProfile> GetCurrentProfileAsync()
      {
            if (HttpContext.Items.TryGetValue(Constants.Keys.ItemsCurrentProfile, out var cachedProfile) && cachedProfile is BusinessProfile profile)
                  return profile;

            var currentProfile = await ProfileService.GetCurrentAsync();
            HttpContext.Items[Constants.Keys.ItemsCurrentProfile] = currentProfile;
            return currentProfile;
      }

      /// <summary>
      /// Automatically Fill ViewBag.CustomLabels
      /// </summary>
      protected async Task SetCustomLabelsAsync()
      {
            var profile = await GetCurrentProfileAsync();
            ViewBag.CustomLabels = profile?.CustomLabels;
      }

      /// <summary>
      /// Fill in all the important ViewBag data
      /// </summary>
      protected async Task SetViewBagDataAsync()
      {
            var profile = await GetCurrentProfileAsync();
            ViewBag.CustomLabels = profile?.CustomLabels;
            ViewBag.BusinessName = profile?.BusinessName;
            ViewBag.PrimaryColor = profile?.Colors?.Primary;
            ViewBag.SecondaryColor = profile?.Colors?.Secondary;
            ViewBag.AccentColor = profile?.Colors?.Accent;
            ViewBag.Currency = profile?.Localization?.Currency ?? Constants.DefaultCurrency;
      }

      /// <summary>
      /// Displaying a success message
      /// </summary>
      protected void SuccessMessage(string message) => MessageService.Success(message);

      /// <summary>
      /// Displaying a error message
      /// </summary>
      protected void ErrorMessage(string message) => MessageService.Error(message);

      /// <summary>
      /// Displaying a warning message
      /// </summary>
      protected void WarningMessage(string message) => MessageService.Warning(message);

      /// <summary>
      /// Validates model using FluentValidator and returns view with errors if invalid
      /// </summary>
      protected async Task<IActionResult> ValidateAndExecuteAsync<TModel, TValidator>(
              TModel model,
              string viewName,
              Func<Task<IActionResult>> onSuccess,
              Func<Task<IActionResult>>? onFailure = null)
              where TValidator : IValidator<TModel>
      {
            var validator = HttpContext.RequestServices.GetRequiredService<TValidator>();
            var validationResult = await validator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                  validationResult.AddToModelState(ModelState);

                  // custom error messages
                  foreach (var error in validationResult.Errors)
                  {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                        WarningMessage(error.ErrorMessage);
                  }

                  // Log validation errors if logger is available
                  var logger = HttpContext.RequestServices.GetService<ILogger<BaseController>>();
                  if (logger != null)
                  {
                        var validationErrors = string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                        logger.LogWarning("Validation failed for {ModelType}: {Errors}", typeof(TModel).Name, validationErrors);
                  }

                  // Execute onFailure callback if provided, otherwise return default view
                  if (onFailure != null)
                  {
                        return await onFailure();
                  }

                  return View(viewName, model);
            }

            return await onSuccess();
      }
}

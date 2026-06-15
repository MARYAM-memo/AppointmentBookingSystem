using AppointmentBooking.Application.DTOs;
using AppointmentBooking.Application.Extensions;
using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.Appointment;
using AppointmentBooking.Core.Interfaces;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Application.Shared;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace AppointmentBooking.Infrastructure.Services;

public class FormPreparationService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IMapper mapper) : IFormPreparationService
{
      private readonly IUnitOfWork _unitOfWork = unitOfWork;
      private readonly IMemoryCache _memoryCache = memoryCache;
      private readonly IMapper _mapper = mapper;
      private static readonly SemaphoreSlim _cacheSemaphore = new(1, 1);

      private readonly string _cacheKey = Constants.Keys.CacheFormSelectList;
      private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

      public async Task<FormDataCacheDTO> GetCachedFormDataAsync()
      {
            if (_memoryCache.TryGetValue(_cacheKey, out FormDataCacheDTO? cachedData) && cachedData is not null)
                  return cachedData;

            await _cacheSemaphore.WaitAsync();
            try
            {
                  if (_memoryCache.TryGetValue(_cacheKey, out cachedData) && cachedData is not null)
                        return cachedData;

                  var activeServices = await GetActiveServicesAsync();
                  var activeCustomers = await GetActiveCustomersAsync();

                  var formData = new FormDataCacheDTO
                  {
                        ServicesList = [.. activeServices.Select(s => new SelectListItem
                        {
                              Value = s.Id.ToString(),
                              Text = $"{s.Name} ({s.Duration:hh\\:mm} - {s.Price.ToCurrency()})"
                        })],

                        CustomersList = [.. activeCustomers.Select(c => new SelectListItem
                        {
                              Value = c.Id.ToString(),
                              Text = $"{c.FullName} ({c.PhoneNumber})"
                        })],

                        ServicesData = activeServices.Select(s => new
                        {
                              id = s.Id,
                              name = s.Name,
                              price = s.Price,
                              duration = s.Duration.TotalMinutes,
                              totalDuration = s.GetTotalDuration().TotalMinutes,
                              icon = s.Icon ?? "bi-stars",
                              color = s.Color ?? ""
                        }).ToList(),

                        ExpiryTime = DateTime.UtcNow.Add(_cacheDuration)
                  };

                  _memoryCache.Set(_cacheKey, formData, _cacheDuration);
                  return formData;
            }
            finally
            {
                  _cacheSemaphore.Release();
            }
      }

      public async Task InvalidateCacheAsync()
      {
            _memoryCache.Remove(_cacheKey);
      }

      public AppointmentRequestViewModel CreateInitialRequest(int? customerId, int? serviceId, DateTime? appointmentDate)
      {
            return new AppointmentRequestViewModel
            {
                  AppointmentDate = appointmentDate ?? DateTime.Today.AddDays(1),
                  Status = BookingStatus.Pending,
                  CustomerId = customerId ?? 0,
                  ServiceId = serviceId ?? 0
            };
      }

      public async Task PopulateViewBagForFormAsync(Controller controller, AppointmentRequestViewModel? model = null, bool isEditMode = false)
      {
            var formData = await GetCachedFormDataAsync();

            controller.ViewBag.Services = formData.ServicesList;
            controller.ViewBag.Customers = formData.CustomersList;
            controller.ViewBag.ServicesData = formData.ServicesData;
            controller.ViewBag.StatusList = BookingStatusExtensions.GetAllStatusesWithLocalizedName();
            controller.ViewBag.IsEdit = isEditMode;
      }

      public async Task PopulateViewBagForListAsync(Controller controller)
      {
            var formData = await GetCachedFormDataAsync();

            controller.ViewBag.Services = formData.ServicesList;
            controller.ViewBag.Customers = formData.CustomersList;
      }

      /// <summary>
      /// Retrieves all active and non-deleted services from the database.
      /// </summary>
      private async Task<IEnumerable<Service>> GetActiveServicesAsync()
      {
            return await _unitOfWork.Services.FetchAsync(
                s => s.IsActive && !s.IsDeleted,
                asNoTracking: true
            );
      }

      /// <summary>
      /// Retrieves all non-deleted customers from the database.
      /// </summary>
      private async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
      {
            return await _unitOfWork.Customers.FetchAsync(
                c => !c.IsDeleted,
                asNoTracking: true
            );
      }
}

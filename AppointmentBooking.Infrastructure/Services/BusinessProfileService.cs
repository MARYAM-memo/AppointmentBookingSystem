using AppointmentBooking.Application.Interfaces;
using AppointmentBooking.Application.ViewModels.BusinessProfile;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Infrastructure.Data;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace AppointmentBooking.Infrastructure.Services;

public class BusinessProfileService(DatabaseContext databaseContext, IMapper mapper) : IBusinessProfileService
{
      private readonly DatabaseContext _context = databaseContext;
      private readonly IMapper _mapper = mapper;
      private static BusinessProfile? _cachedProfile;
      private static readonly Lock _cacheLock = new();
      private static readonly SemaphoreSlim _cacheSemaphore = new(1, 1);
      private static DateTime _cacheExpiry = DateTime.MinValue;
      private const int CacheMinutes = 30;

      public async Task<bool> ExistsAsync() => await _context.BusinessProfiles.AnyAsync();

      public async Task<BusinessProfile> GetCurrentAsync()
      {
            // Fast path: check cache without lock first (read-only check)
            if (_cachedProfile != null && DateTime.UtcNow < _cacheExpiry)
                  return _cachedProfile;

            await _cacheSemaphore.WaitAsync();
            try
            {
                  // Double-check after acquiring lock
                  if (_cachedProfile != null && DateTime.UtcNow < _cacheExpiry)
                        return _cachedProfile;

                  // Cache miss - load from database
                  var profile = _context.BusinessProfiles
                      .AsNoTracking()
                      .OrderBy(c => c.Id)
                      .FirstOrDefault();

                  if (profile == null)
                  {
                        profile = new BusinessProfile();
                        _context.BusinessProfiles.Add(profile);
                        await _context.SaveChangesAsync();
                  }

                  _cachedProfile = profile;
                  _cacheExpiry = DateTime.UtcNow.AddMinutes(CacheMinutes);

                  return profile;
            }
            finally
            {
                  _cacheSemaphore.Release();
            }
      }

      public async Task<BusinessProfile> UpdateAsync(BusinessProfile profile)
      {
            _context.BusinessProfiles.Update(profile);
            await _context.SaveChangesAsync();

            lock (_cacheLock)
            {
                  _cachedProfile = profile;
                  _cacheExpiry = DateTime.UtcNow.AddMinutes(CacheMinutes);
            }

            return profile;
      }

      public async Task<BusinessProfileViewModel> GetCurrentViewModelAsync()
      {
            var profile = await GetCurrentAsync();
            return _mapper.Map<BusinessProfileViewModel>(profile);
      }
}

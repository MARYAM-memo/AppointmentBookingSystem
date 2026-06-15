using AppointmentBooking.Application.ViewModels.BusinessProfile;
using AppointmentBooking.Core.Models;

namespace AppointmentBooking.Application.Interfaces;

public interface IBusinessProfileService
{

      /// <summary>
      /// Retrieves the current business profile with caching (5 minutes expiry), creates a default profile if none exists.
      /// </summary>
      Task<BusinessProfile> GetCurrentAsync();

      /// <summary>
      /// Updates an existing business profile, saves changes, and refreshes the cache.
      /// </summary>
      Task<BusinessProfile> UpdateAsync(BusinessProfile profile);

      /// <summary>
      /// Checks if any business profile exists in the database.
      /// </summary>
      Task<bool> ExistsAsync();

      /// <summary>
      /// Retrieves the current business profile and maps it to a ViewModel asynchronously.
      /// </summary>
      Task<BusinessProfileViewModel> GetCurrentViewModelAsync();
}

using AppointmentBooking.Core.Models;

namespace AppointmentBooking.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
      IRepository<Service> Services { get; }
      IRepository<Appointment> Appointments { get; }
      IRepository<Customer> Customers { get; }

      /// <summary>
      /// Saves all pending changes to the database asynchronously with optional cancellation token.
      /// </summary>
      Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

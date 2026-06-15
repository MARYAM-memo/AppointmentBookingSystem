using AppointmentBooking.Core.Interfaces;
using AppointmentBooking.Core.Models;
using AppointmentBooking.Infrastructure.Data;

namespace AppointmentBooking.Infrastructure.DataAccess;

public class UnitOfWork : IUnitOfWork
{
      readonly DatabaseContext context;
      public UnitOfWork(DatabaseContext ctx)
      {
            context = ctx;
            Services = new Repository<Service>(context);
            Appointments = new Repository<Appointment>(context);
            Customers = new Repository<Customer>(context);
      }

      public IRepository<Service> Services { get; }

      public IRepository<Appointment> Appointments { get; }

      public IRepository<Customer> Customers { get; }

      public void Dispose()
      {
            context.Dispose();
            GC.SuppressFinalize(this);
      }

      public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      {
            return await context.SaveChangesAsync(cancellationToken);
      }
}

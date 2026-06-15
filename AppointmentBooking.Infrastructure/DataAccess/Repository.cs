using System.Linq.Expressions;
using AppointmentBooking.Core.Interfaces;
using AppointmentBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentBooking.Infrastructure.DataAccess;

public class Repository<T>(DatabaseContext databaseContext) : IRepository<T> where T : class
{
      readonly DatabaseContext context = databaseContext;

      /// <summary>
      /// Gets the DbSet for the entity type T from the database context.
      /// </summary>
      DbSet<T> Entities => context.Set<T>();

      public async Task<T?> FindByIdAsync(int id)
      {
            return await Entities.FindAsync(id);
      }

      public async Task<T?> FindFirstOrDefaultAsync<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> orderBy, params Expression<Func<T, object>>[] args)
      {
            IQueryable<T> query = Entities;
            foreach (var arg in args)
            {
                  query = query.Include(arg);
            }
            return await query.OrderBy(orderBy).FirstOrDefaultAsync(predicate);
      }

      public void Add(T entity)
      {
            Entities.Add(entity);
      }

      public void Delete(T entity)
      {
            Entities.Remove(entity);
      }

      public void Update(T entity)
      {
            Entities.Update(entity);
      }

      public async Task<int> CountAsync()
      {
            return await Entities.CountAsync();
      }

      public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
      {
            return await Entities.CountAsync(predicate);
      }

      public async Task<IEnumerable<T>> FetchAsync(bool asNoTracking = false)
      {
            if (asNoTracking) return await Entities.AsNoTracking().ToListAsync();
            else return await Entities.ToListAsync();
      }

      public async Task<IEnumerable<T>> FetchAsync(bool asNoTracking = false, params Expression<Func<T, object>>[] args)
      {
            IQueryable<T> query = Entities;
            foreach (var arg in args)
            {
                  query = query.Include(arg);
            }
            if (asNoTracking) return await query.AsNoTracking().ToListAsync();
            else return await query.ToListAsync();
      }

      public async Task<IEnumerable<T>> FetchAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = false)
      {
            IQueryable<T> query = Entities;
            if (asNoTracking) return await query.Where(predicate).AsNoTracking().ToListAsync();
            else return await query.Where(predicate).ToListAsync();
      }

      public async Task<IEnumerable<T>> FetchAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = false, params Expression<Func<T, object>>[] args)
      {
            IQueryable<T> query = Entities;
            foreach (var arg in args)
            {
                  query = query.Include(arg);
            }

            if (asNoTracking) return await query.Where(predicate).AsNoTracking().ToListAsync();
            else return await query.Where(predicate).ToListAsync();
      }

      public async Task<IEnumerable<T>> FetchAsync(Expression<Func<T, bool>> predicate, int take, bool asNoTracking = false, params Expression<Func<T, object>>[] args)
      {
            IQueryable<T> query = Entities;
            foreach (var arg in args)
            {
                  query = query.Include(arg);
            }

            if (asNoTracking) return await query.Where(predicate).AsNoTracking().Take(take).ToListAsync();
            else return await query.Where(predicate).Take(take).ToListAsync();
      }

      public async Task<IEnumerable<TResult>> FetchAsync<TResult>(Expression<Func<T, TResult>> selector, bool asNoTracking = false, params Expression<Func<T, object>>[] args)
      {
            IQueryable<T> query = Entities;
            foreach (var arg in args)
            {
                  query = query.Include(arg);
            }
            if (asNoTracking) return await query.AsNoTracking().Select(selector).ToListAsync();
            else return await query.Select(selector).ToListAsync();
      }

      public IQueryable<T> GetQueryable(params Expression<Func<T, object>>[] includes)
      {
            IQueryable<T> query = Entities;
            foreach (var include in includes)
            {
                  query = query.Include(include);
            }
            return query;
      }
}

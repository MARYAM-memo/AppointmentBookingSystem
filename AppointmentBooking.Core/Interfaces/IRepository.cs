using System.Linq.Expressions;

namespace AppointmentBooking.Core.Interfaces;

public interface IRepository<T> where T : class
{
      /// <summary>
      /// Finds an entity by its primary key ID asynchronously.
      /// </summary>
      Task<T?> FindByIdAsync(int id);

      /// <summary>
      /// Finds an entity matching a predicate, with includes, ordered by a key selector, asynchronously.
      /// </summary>
      Task<T?> FindFirstOrDefaultAsync<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> orderBy, params Expression<Func<T, object>>[] args);

      /// <summary>
      /// Adds a new entity to the repository for tracking.
      /// </summary>
      void Add(T entity);

      /// <summary>
      /// Marks an entity as modified for update.
      /// </summary>
      void Update(T entity);

      /// <summary>
      /// Marks an entity for deletion.
      /// </summary>
      void Delete(T entity);

      /// <summary>
      /// Returns the total count of entities in the repository asynchronously.
      /// </summary>
      Task<int> CountAsync();

      /// <summary>
      /// Returns the total count of entities that matching a predicate in the repository asynchronously.
      /// </summary>
      Task<int> CountAsync(Expression<Func<T, bool>> predicate);

      /// <summary>
      /// Fetches all entities, optionally with no-tracking for read-only scenarios.
      /// </summary>
      Task<IEnumerable<T>> FetchAsync(bool asNoTracking = false);

      /// <summary>
      /// Fetches all entities with specified navigation properties included, optionally with no-tracking.
      /// </summary>
      Task<IEnumerable<T>> FetchAsync(bool asNoTracking = false, params Expression<Func<T, object>>[] args);

      /// <summary>
      /// Fetches entities matching a predicate, optionally with no-tracking.
      /// </summary>
      Task<IEnumerable<T>> FetchAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = false);

      /// <summary>
      /// Fetches entities matching a predicate with included navigation properties, optionally with no-tracking.
      /// </summary>
      Task<IEnumerable<T>> FetchAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = false, params Expression<Func<T, object>>[] args);

      /// <summary>
      /// Fetches a limited number of entities matching a predicate with includes, optionally with no-tracking.
      /// </summary>
      Task<IEnumerable<T>> FetchAsync(Expression<Func<T, bool>> predicate, int take, bool asNoTracking = false, params Expression<Func<T, object>>[] args);

      /// <summary>
      /// Fetches projected data (selected fields) from entities with optional includes and no-tracking.
      /// </summary>
      Task<IEnumerable<TResult>> FetchAsync<TResult>(Expression<Func<T, TResult>> selector, bool asNoTracking = false, params Expression<Func<T, object>>[] args);

      IQueryable<T> GetQueryable(params Expression<Func<T, object>>[] includes);
}
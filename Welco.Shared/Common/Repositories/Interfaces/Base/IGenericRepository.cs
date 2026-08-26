using System.Linq.Expressions;
using Welco.Shared.Common.Interfaces;

namespace Welco.Shared.Common.Repositories.Interfaces.Base
{
    public interface IGenericRepository<T, TKey>
        where T : class, IBaseEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        // Query methods
        IQueryable<T> GetAll(Expression<Func<T, bool>>? predicate = null);
        IQueryable<T> GetAllAsync(Expression<Func<T, bool>>? predicate = null);
        IQueryable<T> GetBy(Expression<Func<T, bool>> predicate);
        Task<IReadOnlyList<T>> GetAllListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
        Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

        // Include methods
        IQueryable<T> GetAllWithIncluding(Expression<Func<T, bool>>? predicate, params Expression<Func<T, object>>[] includes);
        IQueryable<T> GetFirstWithIncluding(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

        // Filter / Check methods
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<bool> ExistsByKeyAsync(TKey key, CancellationToken cancellationToken = default);
        Task<T?> FindByKeyAsync(TKey key, CancellationToken cancellationToken = default);

        // Add methods
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // Update methods
        void Update(T entity);
        Task UpdateRange(IEnumerable<T> entities);

        // Delete methods
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default);
    }
}

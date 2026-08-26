using Microsoft.EntityFrameworkCore.Storage;
using Welco.Shared.Common.Interfaces;

namespace Welco.Shared.Common.Repositories.Interfaces.Base
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IGenericRepository<T, TKey> GetRepository<T, TKey>()
            where T : class, IBaseEntity<TKey>
            where TKey : IEquatable<TKey>;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}

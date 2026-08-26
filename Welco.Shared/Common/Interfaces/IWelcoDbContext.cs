using Microsoft.EntityFrameworkCore;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Common.Interfaces
{
    public interface IWelcoDbContext : IAsyncDisposable
    {
        DbSet<ApplicationUser> ApplicationUsers { get; }
        DbSet<UserRefreshToken> UserRefreshTokens { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

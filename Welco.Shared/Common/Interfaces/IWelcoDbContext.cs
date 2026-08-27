using Microsoft.EntityFrameworkCore;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Common.Interfaces
{
    public interface IWelcoDbContext : IAsyncDisposable
    {
        DbSet<ApplicationUser> ApplicationUsers { get; }
        DbSet<UserRefreshToken> UserRefreshTokens { get; }
        DbSet<Country> Countries { get; }
        DbSet<City> Cities { get; }
        DbSet<Zone> Zones { get; }
        DbSet<UserAddress> UserAddresses { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

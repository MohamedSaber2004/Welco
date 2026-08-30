using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.Classes;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance
{
    public class WelcoDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid,
        IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, IWelcoDbContext
    {
        private readonly ICurrentUserService? _currentUserService;


        public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
        public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<Zone> Zones => Set<Zone>();
        public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
        public DbSet<Provider> Providers => Set<Provider>();

        public WelcoDbContext(DbContextOptions<WelcoDbContext> options, ICurrentUserService? currentUserService = null)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public override int SaveChanges()
        {
            ApplyAuditInformation();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAuditInformation();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    builder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression<BaseEntity>(e => !e.IsDeleted, entityType.ClrType));
                }
                else if (typeof(ApplicationUser).IsAssignableFrom(entityType.ClrType))
                {
                    builder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression<ApplicationUser>(e => !e.IsDeleted, entityType.ClrType));
                }
            }
        }

        private static LambdaExpression ConvertFilterExpression<TInterface>(
            Expression<Func<TInterface, bool>> filterExpression,
            Type entityType)
        {
            var newParam = Expression.Parameter(entityType);
            var newBody = new ExpressionReplacingVisitor(filterExpression.Parameters.Single(), newParam).Visit(filterExpression.Body);
            return Expression.Lambda(newBody!, newParam);
        }

        private class ExpressionReplacingVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ExpressionReplacingVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression? Visit(Expression? node)
            {
                return node == _oldValue ? _newValue : base.Visit(node);
            }
        }

        private void ApplyAuditInformation()
        {
            var currentUserId = _currentUserService?.UserId != null && _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : (!string.IsNullOrWhiteSpace(_currentUserService?.Email) ? _currentUserService.Email : "System");

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is BaseEntity baseEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            baseEntity.MarkAsCreated(currentUserId);
                            break;

                        case EntityState.Modified:
                            baseEntity.MarkAsUpdated(currentUserId);
                            break;

                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            baseEntity.MarkAsDeleted(currentUserId);
                            break;
                    }
                }
                else if (entry.Entity is ApplicationUser userEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            userEntity.MarkAsCreated(currentUserId);
                            break;

                        case EntityState.Modified:
                            userEntity.MarkAsUpdated(currentUserId);
                            break;

                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            userEntity.MarkAsDeleted(currentUserId);
                            break;
                    }
                }
            }
        }
    }
}

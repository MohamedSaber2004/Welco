using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;

namespace Product.Services.API.Features.Shared
{
    /// <summary>
    /// Mediator model: an OrganizationUser acts for exactly one provider
    /// company. Resolves (IsOrganizationUser, CompanyId) from claims the
    /// same way Sales' BuyerScope does, so product writes can be owned.
    /// </summary>
    internal static class ProviderScope
    {
        public sealed record Caller(bool IsOrganizationUser, Guid? CompanyId);

        public static async Task<Caller> GetAsync(IUnitOfWork uow, ICurrentUserService cur, CancellationToken ct)
        {
            if (cur.UserId == Guid.Empty) return new Caller(false, null);
            var user = await uow.GetRepository<ApplicationUser, Guid>().GetByIdAsync(cur.UserId, ct);
            if (user == null || user.IsDeleted) return new Caller(false, null);
            var isOrg = user.UserType == UserType.OrganizationUser;
            return new Caller(isOrg, isOrg ? user.CompanyId : null);
        }
    }
}

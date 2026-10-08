using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CompanyEntity = Welco.Shared.Domain.Models.Company;

namespace UserManamgent.Service.API.Features.Companies.Commands.DeleteProviderWithCascade
{
    public class DeleteProviderWithCascadeCommandHandler : IRequestHandler<DeleteProviderWithCascadeCommand, Result<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _cur;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteProviderWithCascadeCommandHandler(IUnitOfWork uow, ICurrentUserService cur, UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _cur = cur;
            _userManager = userManager;
        }

        public async Task<Result<string>> Handle(DeleteProviderWithCascadeCommand r, CancellationToken ct)
        {
            var companyRepo = _uow.GetRepository<CompanyEntity, Guid>();
            var company = await companyRepo.GetByIdAsync(r.Id, ct);
            if (company == null || company.IsDeleted)
                return Result<string>.NotFound(LocalizationKeys.Company.NotFound);

            var curId = _cur.UserId != Guid.Empty ? _cur.UserId.ToString() : "System";

            var userRepo = _uow.GetRepository<ApplicationUser, Guid>();
            var userIds = await userRepo.GetAll(u => u.CompanyId == r.Id).Select(u => u.Id).ToListAsync(ct);
            var productRepo = _uow.GetRepository<Product, Guid>();
            var productIds = await productRepo.GetAll(p => p.CompanyId == r.Id).Select(p => p.Id).ToListAsync(ct);

            await _uow.BeginTransactionAsync(ct);
            try
            {
                // ── Hard-delete related data (ExecuteDeleteAsync bypasses the soft-delete interceptor) ──
                // Line items referencing provider products must go first (FK Restrict on Product).
                if (productIds.Count > 0)
                {
                    await _uow.GetRepository<RFQItem, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => productIds.Contains(x.ProductId)).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<QuoteItem, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => productIds.Contains(x.ProductId)).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<OrderItem, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => productIds.Contains(x.ProductId)).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<CartItem, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => productIds.Contains(x.ProductId)).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<ProductInquiry, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => productIds.Contains(x.ProductId)).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<UserProductInteraction, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => productIds.Contains(x.ProductId)).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<ProductMedia, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => productIds.Contains(x.ProductId)).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<ProductSpecification, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => productIds.Contains(x.ProductId)).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<ProductProcedureTag, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => productIds.Contains(x.ProductId)).ExecuteDeleteAsync(ct);
                }

                if (userIds.Count > 0)
                {
                    await _uow.GetRepository<UserAddress, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<UserRefreshToken, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<UserProductInteraction, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<Cart, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => x.UserId.HasValue && userIds.Contains(x.UserId.Value)).ExecuteDeleteAsync(ct);
                    // Provider-owned sales docs (provider as buyer); buyer RFQs/Orders that only
                    // referenced provider products keep their parents — only line items above were removed.
                    await _uow.GetRepository<Quote, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => x.RFQ != null && x.RFQ.CompanyId == r.Id).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<RFQ, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => x.CompanyId == r.Id).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<Invoice, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => x.Order != null && ((x.Order.UserId.HasValue && userIds.Contains(x.Order.UserId.Value)) || x.Order.CompanyId == r.Id)).ExecuteDeleteAsync(ct);
                    await _uow.GetRepository<Order, Guid>().GetAll().IgnoreQueryFilters()
                        .Where(x => (x.UserId.HasValue && userIds.Contains(x.UserId.Value)) || x.CompanyId == r.Id).ExecuteDeleteAsync(ct);
                }

                await _uow.GetRepository<CompanyAddress, Guid>().GetAll().IgnoreQueryFilters()
                    .Where(x => x.CompanyId == r.Id).ExecuteDeleteAsync(ct);
                if (productIds.Count > 0)
                {
                    await productRepo.GetAll().IgnoreQueryFilters()
                        .Where(x => productIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
                }

                // ── Soft-delete users + provider/company (stay queryable for audit/history) ──
                var users = await userRepo.GetAll(u => u.CompanyId == r.Id).ToListAsync(ct);
                foreach (var u in users)
                {
                    u.MarkAsDeleted(curId);
                    var res = await _userManager.UpdateAsync(u);
                    if (!res.Succeeded)
                        return Result<string>.BadRequest(res.Errors.FirstOrDefault()?.Description ?? LocalizationKeys.ExceptionMessages.BadRequest);
                }

                company.MarkAsDeleted(curId);
                companyRepo.Update(company);

                await _uow.SaveChangesAsync(ct);
                await _uow.CommitAsync(ct);
                return Result<string>.Success(company.Id.ToString(), LocalizationKeys.Company.Deleted);
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }
    }
}

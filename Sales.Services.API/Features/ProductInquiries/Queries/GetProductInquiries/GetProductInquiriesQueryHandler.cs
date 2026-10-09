using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
namespace Sales.Services.API.Features.ProductInquiries.Queries.GetProductInquiries
{
    public class GetProductInquiriesQueryHandler : IRequestHandler<GetProductInquiriesQuery, PaginatedResult<ProductInquiryDto>>
    {
        private readonly IUnitOfWork _uow; private readonly ICurrentUserService _cur;
        public GetProductInquiriesQueryHandler(IUnitOfWork uow, ICurrentUserService cur) { _uow = uow; _cur = cur; }
        public async Task<PaginatedResult<ProductInquiryDto>> Handle(GetProductInquiriesQuery r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<ProductInquiry, Guid>();
            var q = repo.GetAll(x => !x.IsDeleted).AsNoTracking();
            var caller = await Sales.Services.API.Features.Shared.BuyerScope.GetAsync(_uow, _cur, ct);
            if (caller.IsOrganizationUser && caller.CompanyId.HasValue)
            {
                var companyId = caller.CompanyId.Value;
                q = q.Where(x => x.Product != null && x.Product.CompanyId == companyId);
            }
            else if (!caller.IsOrganizationUser && _cur.UserId != Guid.Empty)
            {
                var uid = _cur.UserId;
                var uidStr = uid.ToString();
                var curEmail = !string.IsNullOrWhiteSpace(_cur.Email) ? _cur.Email.ToLower() : null;
                q = q.Where(x => x.UserId == uid || x.CreatedBy == uidStr || (curEmail != null && x.Email != null && x.Email.ToLower() == curEmail));
            }
            if (r.ProductId.HasValue && r.ProductId.Value != Guid.Empty) q = q.Where(x => x.ProductId == r.ProductId.Value);
            if (!string.IsNullOrWhiteSpace(r.SearchTerm))
            {
                var term = r.SearchTerm.Trim().ToLower();
                q = q.Where(x => x.Name.ToLower().Contains(term) || x.Organization.ToLower().Contains(term) || (x.Email != null && x.Email.ToLower().Contains(term)) || x.Message.ToLower().Contains(term));
            }
            return await q.OrderByDescending(x => x.CreatedAt).ToPaginatedListAsync(x => new ProductInquiryDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductNameEn = x.Product != null ? x.Product.NameEn : null,
                ProductNameAr = x.Product != null ? x.Product.NameAr : null,
                ProductSku = x.Product != null ? x.Product.Sku : null,
                Name = x.Name,
                Organization = x.Organization,
                Message = x.Message,
                Email = x.Email,
                Status = x.Status.ToString(),
                UserId = x.UserId,
                Response = x.Response,
                RespondedAt = x.RespondedAt,
                RespondedById = x.RespondedById,
                CreatedAt = x.CreatedAt
            }, r.PageNumber, r.PageSize, LocalizationKeys.ProductInquiry.ListFetched, ct);
        }
    }
}

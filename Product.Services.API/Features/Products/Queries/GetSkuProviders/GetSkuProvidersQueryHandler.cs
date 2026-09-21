using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Services.API.Common;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Products.Queries.GetSkuProviders
{
    public class GetSkuProvidersQueryHandler : IRequestHandler<GetSkuProvidersQuery, PaginatedResult<SkuProviderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetSkuProvidersQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<PaginatedResult<SkuProviderDto>> Handle(GetSkuProvidersQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            var sku = request.Sku.Trim().ToLower();

            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var matches = await productRepo
                .GetAll(p => !p.IsDeleted && p.IsActive && p.CompanyId != null && p.Sku.ToLower() == sku)
                .Select(ProductDtoMapper.Projection)
                .ToListAsync(cancellationToken);

            var ownerIds = matches
                .Select(m => m.CompanyId!.Value)
                .Distinct()
                .ToList();

            var byOwner = new Dictionary<Guid, CompanyDto>();
            if (ownerIds.Count > 0)
            {
                var companyRepo = _unitOfWork.GetRepository<Company, Guid>();
                var companies = await companyRepo
                    .GetAll(c => !c.IsDeleted && c.IsActive && c.Status == CompanyStatus.Approved && ownerIds.Contains(c.Id))
                    .Select(c => new CompanyDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        ImageName = c.ImageName,
                        Type = c.Type,
                        CountryId = c.CountryId,
                        CountryNameEn = c.Country != null ? c.Country.NameEn : null,
                        CountryNameAr = c.Country != null ? c.Country.NameAr : null,
                        Status = c.Status,
                        AccountManagerId = c.AccountManagerId,
                        IsActive = c.IsActive,
                        IsProvider = c.IsProvider,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync(cancellationToken);
                foreach (var c in companies) byOwner[c.Id] = c;
            }

            var items = matches
                .Where(m => m.CompanyId.HasValue && byOwner.ContainsKey(m.CompanyId.Value))
                .GroupBy(m => m.CompanyId!.Value)
                .Select(g => new SkuProviderDto { Company = byOwner[g.Key], Listing = g.OrderByDescending(l => l.CreatedAt).First() })
                .OrderBy(x => x.Company!.Name)
                .ToList();

            return PaginatedResult<SkuProviderDto>.Success(
                items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                items.Count,
                pageNumber,
                pageSize,
                LocalizationKeys.Product.ListFetched);
        }
    }
}

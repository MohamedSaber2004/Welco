using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Companies.Queries.GetCompanyProducts
{
    public class GetCompanyProductsQueryHandler : IRequestHandler<GetCompanyProductsQuery, PaginatedResult<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetCompanyProductsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<PaginatedResult<ProductDto>> Handle(GetCompanyProductsQuery request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.GetRepository<Product, Guid>();
            var query = productRepo.GetAll(p => !p.IsDeleted && p.IsActive && p.CompanyId == request.CompanyId);

            if (request.CategoryId.HasValue)
            {
                var targetCatId = request.CategoryId.Value;
                query = query.Where(p => p.CategoryId == targetCatId || (p.Category != null && p.Category.ParentCategoryId == targetCatId));
            }

            query = query.OrderByDescending(p => p.CreatedAt);

            return await query.ToPaginatedListAsync(p => new ProductDto
            {
                Id = p.Id,
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                Sku = p.Sku,
                Slug = p.Slug,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                Specifications = p.Specifications,
                ImageName = p.ImageName,
                Material = p.Material,
                LengthCm = p.LengthCm,
                CurrencyId = p.CurrencyId,
                CurrencyCode = p.Currency != null ? p.Currency.Code : null,
                CurrencySymbol = p.Currency != null ? p.Currency.Symbol : null,
                CategoryId = p.CategoryId,
                CategoryNameEn = p.Category != null ? p.Category.NameEn : null,
                CategoryNameAr = p.Category != null ? p.Category.NameAr : null,
                CompanyId = p.CompanyId,
                CompanyName = p.Company != null ? p.Company.Name : null,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }, request.PageNumber, request.PageSize, LocalizationKeys.Product.ListFetched, cancellationToken);
        }
    }
}

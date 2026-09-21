using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Categories.Queries.GetCategoryProviders
{
    public class GetCategoryProvidersQueryHandler : IRequestHandler<GetCategoryProvidersQuery, PaginatedResult<CompanyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetCategoryProvidersQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<PaginatedResult<CompanyDto>> Handle(GetCategoryProvidersQuery request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var ownerIds = await productRepo
                .GetAll(p => !p.IsDeleted && p.IsActive && p.CompanyId != null &&
                    (p.CategoryId == request.CategoryId || (p.Category != null && p.Category.ParentCategoryId == request.CategoryId)))
                .Select(p => p.CompanyId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (ownerIds.Count == 0)
            {
                return PaginatedResult<CompanyDto>.Success(
                    new List<CompanyDto>(), 0,
                    request.PageNumber <= 0 ? 1 : request.PageNumber,
                    request.PageSize <= 0 ? 10 : request.PageSize,
                    LocalizationKeys.Company.ListFetched);
            }

            var companyRepo = _unitOfWork.GetRepository<Company, Guid>();
            var query = companyRepo
                .GetAll(c => !c.IsDeleted && c.IsActive && c.Status == CompanyStatus.Approved && ownerIds.Contains(c.Id))
                .OrderBy(c => c.Name);

            return await query.ToPaginatedListAsync(c => new CompanyDto
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
            }, request.PageNumber, request.PageSize, LocalizationKeys.Company.ListFetched, cancellationToken);
        }
    }
}

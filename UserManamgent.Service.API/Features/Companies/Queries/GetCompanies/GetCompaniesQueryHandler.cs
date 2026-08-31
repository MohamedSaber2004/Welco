using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CompanyEntity = Welco.Shared.Domain.Models.Company;

namespace UserManamgent.Service.API.Features.Companies.Queries.GetCompanies
{
    public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, PaginatedResult<CompanyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetCompaniesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public async Task<PaginatedResult<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<CompanyEntity, Guid>();
            var query = repo.GetAll(c => !c.IsDeleted).AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(term));
            }
            if (request.IsActive.HasValue) query = query.Where(c => c.IsActive == request.IsActive.Value);
            return await query.OrderBy(c => c.Name).ToPaginatedListAsync(c => new CompanyDto
            {
                Id = c.Id, Name = c.Name, Type = c.Type, CountryId = c.CountryId, CountryNameEn = c.Country != null ? c.Country.NameEn : null, TierLevel = c.TierLevel, Status = c.Status, AccountManagerId = c.AccountManagerId, IsActive = c.IsActive, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
            }, request.PageNumber, request.PageSize, LocalizationKeys.Company.ListFetched, cancellationToken);
        }
    }
}

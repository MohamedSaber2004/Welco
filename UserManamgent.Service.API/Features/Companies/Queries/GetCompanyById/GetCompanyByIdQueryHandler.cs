using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CompanyEntity = Welco.Shared.Domain.Models.Company;
namespace UserManamgent.Service.API.Features.Companies.Queries.GetCompanyById
{
    public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, Result<CompanyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetCompanyByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        public async Task<Result<CompanyDto>> Handle(GetCompanyByIdQuery request, CancellationToken ct)
        {
            var repo = _unitOfWork.GetRepository<CompanyEntity, Guid>();
            var c = await repo.GetByIdAsync(request.Id, ct);
            if (c == null || c.IsDeleted) return Result<CompanyDto>.NotFound(LocalizationKeys.Company.NotFound);
            return Result<CompanyDto>.Success(new CompanyDto { Id = c.Id, Name = c.Name, Type = c.Type, CountryId = c.CountryId, TierLevel = c.TierLevel, Status = c.Status, AccountManagerId = c.AccountManagerId, IsActive = c.IsActive, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt }, LocalizationKeys.Company.Fetched);
        }
    }
}

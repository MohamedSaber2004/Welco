using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CompanyEntity = Welco.Shared.Domain.Models.Company;
namespace UserManamgent.Service.API.Features.Companies.Commands.UpdateCompany
{
    public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, Result<CompanyDto>>
    {
        private readonly IUnitOfWork _uow; private readonly ICurrentUserService _cur;
        public UpdateCompanyCommandHandler(IUnitOfWork uow, ICurrentUserService cur) { _uow = uow; _cur = cur; }
        public async Task<Result<CompanyDto>> Handle(UpdateCompanyCommand r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<CompanyEntity, Guid>();
            var c = await repo.GetByIdAsync(r.Id, ct);
            if (c == null || c.IsDeleted) return Result<CompanyDto>.NotFound(LocalizationKeys.Company.NotFound);
            var countryRepo = _uow.GetRepository<Welco.Shared.Domain.Models.Country, Guid>();
            if (!await countryRepo.ExistsAsync(x => !x.IsDeleted && x.Id == r.CountryId, ct)) return Result<CompanyDto>.BadRequest(LocalizationKeys.Company.CountryRequired);
            var curId = _cur.UserId != Guid.Empty ? _cur.UserId.ToString() : "System";
            c.Update(r.Name.Trim(), r.Type, r.CountryId, r.TierLevel, r.Status, r.AccountManagerId, curId);
            if (r.IsActive.HasValue) c.SetActiveState(r.IsActive.Value, curId);
            repo.Update(c); await _uow.SaveChangesAsync(ct);
            return Result<CompanyDto>.Success(new CompanyDto { Id = c.Id, Name = c.Name, Type = c.Type, CountryId = c.CountryId, TierLevel = c.TierLevel, Status = c.Status, AccountManagerId = c.AccountManagerId, IsActive = c.IsActive, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt }, LocalizationKeys.Company.Updated);
        }
    }
}

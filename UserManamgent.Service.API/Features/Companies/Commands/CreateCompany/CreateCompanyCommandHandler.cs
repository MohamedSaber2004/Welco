using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CompanyEntity = Welco.Shared.Domain.Models.Company;
namespace UserManamgent.Service.API.Features.Companies.Commands.CreateCompany
{
    public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Result<CompanyDto>>
    {
        private readonly IUnitOfWork _uow; private readonly ICurrentUserService _cur;
        public CreateCompanyCommandHandler(IUnitOfWork uow, ICurrentUserService cur) { _uow = uow; _cur = cur; }
        public async Task<Result<CompanyDto>> Handle(CreateCompanyCommand r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<CompanyEntity, Guid>();
            var countryRepo = _uow.GetRepository<Welco.Shared.Domain.Models.Country, Guid>();
            if (!await countryRepo.ExistsAsync(c => !c.IsDeleted && c.Id == r.CountryId, ct)) return Result<CompanyDto>.BadRequest(LocalizationKeys.Company.CountryRequired);
            var curId = _cur.UserId != Guid.Empty ? _cur.UserId.ToString() : "System";
            var c = CompanyEntity.Create(r.Name.Trim(), r.Type, r.CountryId, r.TierLevel, r.Status, r.AccountManagerId, curId);
            await repo.AddAsync(c, ct); await _uow.SaveChangesAsync(ct);
            return Result<CompanyDto>.Created(new CompanyDto { Id = c.Id, Name = c.Name, Type = c.Type, CountryId = c.CountryId, TierLevel = c.TierLevel, Status = c.Status, AccountManagerId = c.AccountManagerId, IsActive = c.IsActive, CreatedAt = c.CreatedAt }, LocalizationKeys.Company.Created);
        }
    }
}

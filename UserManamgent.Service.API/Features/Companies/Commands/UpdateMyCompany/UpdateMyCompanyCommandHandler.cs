using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CompanyEntity = Welco.Shared.Domain.Models.Company;

namespace UserManamgent.Service.API.Features.Companies.Commands.UpdateMyCompany
{
    public class UpdateMyCompanyCommandHandler : IRequestHandler<UpdateMyCompanyCommand, Result<CompanyDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _cur;

        public UpdateMyCompanyCommandHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork uow,
            ICurrentUserService cur)
        {
            _userManager = userManager;
            _uow = uow;
            _cur = cur;
        }

        public async Task<Result<CompanyDto>> Handle(UpdateMyCompanyCommand r, CancellationToken ct)
        {
            if (!_cur.IsAuthenticated || _cur.UserId == Guid.Empty)
                return Result<CompanyDto>.Unauthorized(LocalizationKeys.ExceptionMessages.Unauthorized);

            var user = await _userManager.FindByIdAsync(_cur.UserId.ToString());
            if (user == null || user.IsDeleted)
                return Result<CompanyDto>.NotFound(LocalizationKeys.UserManagement.UserNotFound);

            // A user with no linked organization has nothing to self-update —
            // they must go through the distributor application flow instead.
            if (!user.CompanyId.HasValue)
                return Result<CompanyDto>.NotFound(LocalizationKeys.Company.NotFound);

            var repo = _uow.GetRepository<CompanyEntity, Guid>();
            var company = await repo.GetByIdAsync(user.CompanyId.Value, ct);
            if (company == null || company.IsDeleted)
                return Result<CompanyDto>.NotFound(LocalizationKeys.Company.NotFound);

            var countryRepo = _uow.GetRepository<Country, Guid>();
            if (!await countryRepo.ExistsAsync(x => !x.IsDeleted && x.Id == r.CountryId, ct))
                return Result<CompanyDto>.BadRequest(LocalizationKeys.Company.CountryRequired);

            var curId = _cur.UserId != Guid.Empty ? _cur.UserId.ToString() : "System";

            // Status / Type / AccountManagerId are passed through from the stored
            // record — the caller cannot influence them. IsActive is likewise left
            // untouched because Company.Update never mutates it.
            company.Update(
                r.Name.Trim(),
                company.Type,
                r.CountryId,
                company.Status,
                company.AccountManagerId,
                curId,
                r.Email,
                r.ImageName);

            repo.Update(company);
            await _uow.SaveChangesAsync(ct);

            string? countryNameEn = null;
            string? countryNameAr = null;
            try
            {
                var country = await countryRepo.GetByIdAsync(company.CountryId, ct);
                if (country != null)
                {
                    countryNameEn = country.NameEn;
                    countryNameAr = country.NameAr;
                }
            }
            catch { }

            return Result<CompanyDto>.Success(new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Email = company.Email,
                ImageName = company.ImageName,
                Type = company.Type,
                CountryId = company.CountryId,
                CountryNameEn = countryNameEn,
                CountryNameAr = countryNameAr,
                Status = company.Status,
                AccountManagerId = company.AccountManagerId,
                IsActive = company.IsActive,
                IsProvider = company.IsProvider,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
            }, LocalizationKeys.Company.Updated);
        }
    }
}

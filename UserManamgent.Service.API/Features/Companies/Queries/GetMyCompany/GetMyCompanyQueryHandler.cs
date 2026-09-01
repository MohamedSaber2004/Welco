using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using Welco.Shared.Common.Interfaces;

namespace UserManamgent.Service.API.Features.Companies.Queries.GetMyCompany
{
    public class GetMyCompanyQueryHandler : IRequestHandler<GetMyCompanyQuery, Result<CompanyDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetMyCompanyQueryHandler(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CompanyDto>> Handle(GetMyCompanyQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId == Guid.Empty)
                return Result<CompanyDto>.Unauthorized(LocalizationKeys.ExceptionMessages.Unauthorized);

            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
            if (user == null || user.IsDeleted)
                return Result<CompanyDto>.NotFound(LocalizationKeys.UserManagement.UserNotFound);

            if (!user.CompanyId.HasValue)
                return Result<CompanyDto>.NotFound(LocalizationKeys.Company.NotFound);

            var repo = _unitOfWork.GetRepository<Company, Guid>();
            var company = await repo.GetByIdAsync(user.CompanyId.Value, cancellationToken);
            if (company == null || company.IsDeleted)
                return Result<CompanyDto>.NotFound(LocalizationKeys.Company.NotFound);

            var dto = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Type = company.Type,
                CountryId = company.CountryId,
                TierLevel = company.TierLevel,
                Status = company.Status,
                AccountManagerId = company.AccountManagerId,
                IsActive = company.IsActive,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
            };

            // Try to populate CountryNameEn for convenience (optional)
            try
            {
                var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                var country = await countryRepo.GetByIdAsync(company.CountryId, cancellationToken);
                if (country != null) dto.CountryNameEn = country.NameEn;
            }
            catch { /* ignore */ }

            return Result<CompanyDto>.Success(dto, LocalizationKeys.Company.Fetched);
        }
    }
}

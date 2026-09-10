using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using Welco.Shared.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            {
                if (user.UserType == UserType.OrganizationUser)
                {
                    try
                    {
                        var distRepo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
                        var userEmail = (user.Email ?? "").Trim().ToLower();
                        var app = await distRepo.GetAll(d => !d.IsDeleted && (d.ContactEmail.ToLower() == userEmail || d.CreatedBy.ToLower() == userEmail))
                            .OrderByDescending(d => d.CreatedAt)
                            .FirstOrDefaultAsync(cancellationToken);
                        if (app != null)
                        {
                            string? countryNameEn = null;
                            string? countryNameAr = null;
                            try
                            {
                                var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                                var country = await countryRepo.GetByIdAsync(app.CountryId, cancellationToken);
                                countryNameEn = country?.NameEn;
                                countryNameAr = country?.NameAr;
                            }
                            catch { }

                            var appDto = new CompanyDto
                            {
                                Id = app.Id,
                                Name = app.CompanyName,
                                Email = app.ContactEmail,
                                Type = app.Type,
                                CountryId = app.CountryId,
                                CountryNameEn = countryNameEn,
                                CountryNameAr = countryNameAr,
                                Status = app.Status == DistributorApplicationStatus.Approved ? CompanyStatus.Approved :
                                         app.Status == DistributorApplicationStatus.Rejected ? CompanyStatus.Rejected :
                                         CompanyStatus.Pending,
                                CreatedAt = app.CreatedAt,
                                UpdatedAt = app.UpdatedAt
                            };
                            return Result<CompanyDto>.Success(appDto, LocalizationKeys.Company.Fetched);
                        }
                    }
                    catch { }
                }

                return Result<CompanyDto>.NotFound(LocalizationKeys.Company.NotFound);
            }

            var repo = _unitOfWork.GetRepository<Company, Guid>();
            var company = await repo.GetByIdAsync(user.CompanyId.Value, cancellationToken);
            if (company == null || company.IsDeleted)
                return Result<CompanyDto>.NotFound(LocalizationKeys.Company.NotFound);

            var dto = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Email = company.Email,
                ImageName = company.ImageName,
                Type = company.Type,
                CountryId = company.CountryId,
                Status = company.Status,
                AccountManagerId = company.AccountManagerId,
                IsActive = company.IsActive,
                IsProvider = company.IsProvider,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
            };

try
            {
                var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                var country = await countryRepo.GetByIdAsync(company.CountryId, cancellationToken);
                if (country != null)
                {
                    dto.CountryNameEn = country.NameEn;
                    dto.CountryNameAr = country.NameAr;
                }
            }
            catch {  }

            return Result<CompanyDto>.Success(dto, LocalizationKeys.Company.Fetched);
        }
    }
}

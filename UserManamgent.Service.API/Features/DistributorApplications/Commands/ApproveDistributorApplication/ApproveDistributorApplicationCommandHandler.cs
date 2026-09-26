using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.DistributorApplications.Commands.ApproveDistributorApplication
{
    public class ApproveDistributorApplicationCommandHandler : IRequestHandler<ApproveDistributorApplicationCommand, Result<DistributorApplicationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ApproveDistributorApplicationCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<DistributorApplicationDto>> Handle(ApproveDistributorApplicationCommand request, CancellationToken cancellationToken)
        {
            var appRepo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
            var app = await appRepo.GetAll(a => a.Id == request.Id && !a.IsDeleted)
                .Include(a => a.Country)
                .FirstOrDefaultAsync(cancellationToken);

            if (app == null)
            {
                return Result<DistributorApplicationDto>.NotFound(LocalizationKeys.DistributorApplication.NotFound);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

if (app.Status == DistributorApplicationStatus.Approved)
            {
                var healCompanyRepo = _unitOfWork.GetRepository<Company, Guid>();
                var approvedCompany = await healCompanyRepo.GetAll(c => c.Name.ToLower() == app.CompanyName.ToLower() && !c.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (approvedCompany != null
                    && approvedCompany.Status == CompanyStatus.Approved)
                {
                    var (healModified, healApplicant) = await TryLinkApplicantAsync(app, approvedCompany.Id, currentUserId, cancellationToken);
                    if (healModified)
                    {
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        return Result<DistributorApplicationDto>.Success(ToDto(app, healApplicant), LocalizationKeys.DistributorApplication.Approved);
                    }
                }

                return Result<DistributorApplicationDto>.BadRequest(LocalizationKeys.DistributorApplication.AlreadyProcessed);
            }

            app.Status = DistributorApplicationStatus.Approved;
            app.MarkAsUpdated(currentUserId);

var companyRepo = _unitOfWork.GetRepository<Company, Guid>();
            var existingCompany = await companyRepo.GetAll(c => c.Name.ToLower() == app.CompanyName.ToLower() && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            Guid companyId;
            if (existingCompany != null)
            {
                existingCompany.Update(
                    existingCompany.Name,
                    app.Type,
                    app.CountryId,
                    CompanyStatus.Approved,
                    request.AccountManagerId,
                    currentUserId,
                    string.IsNullOrWhiteSpace(existingCompany.Email) ? app.ContactEmail : existingCompany.Email);
                companyId = existingCompany.Id;
            }
            else
            {
                var newCompany = Company.Create(
                    app.CompanyName,
                    app.Type,
                    app.CountryId,
                    CompanyStatus.Approved,
                    request.AccountManagerId,
                    currentUserId,
                    app.ContactEmail);
                await companyRepo.AddAsync(newCompany, cancellationToken);
                companyId = newCompany.Id;
            }

            var (_, linkedApplicant) = await TryLinkApplicantAsync(app, companyId, currentUserId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<DistributorApplicationDto>.Success(ToDto(app, linkedApplicant), LocalizationKeys.DistributorApplication.Approved);
        }

        private async Task<(bool Modified, ApplicationUser? Applicant)> TryLinkApplicantAsync(DistributorApplication app, Guid companyId, string currentUserId, CancellationToken cancellationToken)
        {
            var userRepo = _unitOfWork.GetRepository<ApplicationUser, Guid>();
            ApplicationUser? applicant = null;

            if (!string.IsNullOrWhiteSpace(app.CreatedBy))
            {
                if (Guid.TryParse(app.CreatedBy, out var createdById) && createdById != Guid.Empty)
                {
                    applicant = await userRepo.GetByIdAsync(createdById, cancellationToken);
                }
                else if (app.CreatedBy.Contains('@'))
                {
                    var createdByEmail = app.CreatedBy.Trim().ToLower();
                    applicant = await userRepo.GetAll(u => !u.IsDeleted && (u.Email ?? "").ToLower() == createdByEmail)
                        .FirstOrDefaultAsync(cancellationToken);
                }
            }

            if ((applicant == null || applicant.IsDeleted) && !string.IsNullOrWhiteSpace(app.ContactEmail) && app.ContactEmail.Contains('@'))
            {
                var email = app.ContactEmail.Trim().ToLower();
                applicant = await userRepo.GetAll(u => !u.IsDeleted && (u.Email ?? "").ToLower() == email)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (applicant == null || applicant.IsDeleted)
                return (false, null);

            var modified = false;
            if (applicant.CompanyId != companyId)
            {
                applicant.CompanyId = companyId;
                modified = true;
            }
            if (applicant.UserType != UserType.OrganizationUser)
            {
                applicant.UserType = UserType.OrganizationUser;
                modified = true;
            }
            if (!applicant.IsActive)
            {
                applicant.IsActive = true;
                modified = true;
            }
            if (!applicant.EmailConfirmed)
            {
                applicant.EmailConfirmed = true;
                modified = true;
            }
            if (modified)
            {
                applicant.MarkAsUpdated(currentUserId);
            }
            return (modified, applicant);
        }

        private static DistributorApplicationDto ToDto(DistributorApplication app, ApplicationUser? applicant = null)
        {
            return new DistributorApplicationDto
            {
                Id = app.Id,
                CompanyName = app.CompanyName,
                Type = app.Type,
                CountryId = app.CountryId,
                CountryNameEn = app.Country != null ? app.Country.NameEn : null,
                SalesVolumeBand = app.SalesVolumeBand,
                CategoryInterest = app.CategoryInterest,
                Website = app.Website,
                ContactPerson = app.ContactPerson,
                ContactEmail = app.ContactEmail,
                Phone = app.Phone,
                Status = app.Status.ToString(),
                CreatedAt = app.CreatedAt,
                UpdatedAt = app.UpdatedAt,
                ApplicantUser = applicant != null && !applicant.IsDeleted
                    ? new ApplicantUserDto
                    {
                        Id = applicant.Id,
                        FullName = applicant.FullName,
                        Email = applicant.Email ?? string.Empty,
                        PhoneNumber = applicant.PhoneNumber,
                        UserType = applicant.UserType,
                        IsActive = applicant.IsActive,
                        EmailConfirmed = applicant.EmailConfirmed,
                        CreatedAt = applicant.CreatedAt
                    }
                    : null
            };
        }
    }
}

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

            // Heal path: applications approved before the applicant-link fix left
            // User.CompanyId null. Re-running approve attaches the applicant to
            // the approved company instead of failing with AlreadyProcessed.
            if (app.Status == DistributorApplicationStatus.Approved)
            {
                var healCompanyRepo = _unitOfWork.GetRepository<Company, Guid>();
                var approvedCompany = await healCompanyRepo.GetAll(c => c.Name.ToLower() == app.CompanyName.ToLower() && !c.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (approvedCompany != null
                    && approvedCompany.Status == CompanyStatus.Approved
                    && await TryLinkApplicantAsync(app, approvedCompany.Id, currentUserId, cancellationToken))
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return Result<DistributorApplicationDto>.Success(ToDto(app), LocalizationKeys.DistributorApplication.Approved);
                }

                return Result<DistributorApplicationDto>.BadRequest(LocalizationKeys.DistributorApplication.AlreadyProcessed);
            }

            app.Status = DistributorApplicationStatus.Approved;
            app.MarkAsUpdated(currentUserId);

            // Create or update the Company record
            var companyRepo = _unitOfWork.GetRepository<Company, Guid>();
            var existingCompany = await companyRepo.GetAll(c => c.Name.ToLower() == app.CompanyName.ToLower() && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            Guid companyId;
            if (existingCompany != null)
            {
                existingCompany.Update(
                    existingCompany.Name,
                    CompanyType.Distributor,
                    app.CountryId,
                    request.TierLevel > 0 ? request.TierLevel : 1,
                    CompanyStatus.Approved,
                    request.AccountManagerId,
                    currentUserId);
                companyId = existingCompany.Id;
            }
            else
            {
                var newCompany = Company.Create(
                    app.CompanyName,
                    CompanyType.Distributor,
                    app.CountryId,
                    request.TierLevel > 0 ? request.TierLevel : 1,
                    CompanyStatus.Approved,
                    request.AccountManagerId,
                    currentUserId);
                await companyRepo.AddAsync(newCompany, cancellationToken);
                companyId = newCompany.Id;
            }

            // Link applicant user if available
            await TryLinkApplicantAsync(app, companyId, currentUserId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<DistributorApplicationDto>.Success(ToDto(app), LocalizationKeys.DistributorApplication.Approved);
        }

        /// <summary>
        /// Attaches the applicant user to the approved company. Registration-created
        /// applications store the applicant email in CreatedBy, while staff-created
        /// ones store the staff user id — both shapes are resolved here.
        /// Returns true when a missing link was applied.
        /// </summary>
        private async Task<bool> TryLinkApplicantAsync(DistributorApplication app, Guid companyId, string currentUserId, CancellationToken cancellationToken)
        {
            var userRepo = _unitOfWork.GetRepository<ApplicationUser, Guid>();
            ApplicationUser? applicant = null;

            if (!string.IsNullOrWhiteSpace(app.CreatedBy)
                && Guid.TryParse(app.CreatedBy, out var createdById)
                && createdById != Guid.Empty)
            {
                applicant = await userRepo.GetByIdAsync(createdById, cancellationToken);
            }

            if ((applicant == null || applicant.IsDeleted) && !string.IsNullOrWhiteSpace(app.ContactEmail))
            {
                var email = app.ContactEmail.Trim().ToLower();
                applicant = await userRepo.GetAll(u => !u.IsDeleted && (u.Email ?? "").ToLower() == email)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (applicant == null || applicant.IsDeleted || applicant.CompanyId == companyId)
                return false;

            applicant.CompanyId = companyId;
            applicant.MarkAsUpdated(currentUserId);
            return true;
        }

        private static DistributorApplicationDto ToDto(DistributorApplication app)
        {
            return new DistributorApplicationDto
            {
                Id = app.Id,
                CompanyName = app.CompanyName,
                CountryId = app.CountryId,
                CountryNameEn = app.Country != null ? app.Country.NameEn : null,
                SalesVolumeBand = app.SalesVolumeBand,
                Website = app.Website,
                ContactPerson = app.ContactPerson,
                ContactEmail = app.ContactEmail,
                Status = app.Status.ToString(),
                CreatedAt = app.CreatedAt,
                UpdatedAt = app.UpdatedAt
            };
        }
    }
}

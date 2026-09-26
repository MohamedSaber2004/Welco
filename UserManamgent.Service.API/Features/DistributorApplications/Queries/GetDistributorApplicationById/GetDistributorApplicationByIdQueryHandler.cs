using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.DistributorApplications.Queries.GetDistributorApplicationById
{
    public class GetDistributorApplicationByIdQueryHandler : IRequestHandler<GetDistributorApplicationByIdQuery, Result<DistributorApplicationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDistributorApplicationByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DistributorApplicationDto>> Handle(GetDistributorApplicationByIdQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
            var app = await repo.GetAll(a => a.Id == request.Id && !a.IsDeleted)
                .Include(a => a.Country)
                .FirstOrDefaultAsync(cancellationToken);

            if (app == null)
            {
                return Result<DistributorApplicationDto>.NotFound(LocalizationKeys.DistributorApplication.NotFound);
            }

            var userRepo = _unitOfWork.GetRepository<ApplicationUser, Guid>();
            ApplicationUser? applicantUser = null;

            // 1. If CreatedBy is a valid Guid, lookup by Id
            if (!string.IsNullOrWhiteSpace(app.CreatedBy) && Guid.TryParse(app.CreatedBy.Trim(), out var createdById) && createdById != Guid.Empty)
            {
                applicantUser = await userRepo.GetAll(u => !u.IsDeleted && u.Id == createdById)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            // 2. If CreatedBy is an email, lookup by NormalizedEmail
            if (applicantUser == null && !string.IsNullOrWhiteSpace(app.CreatedBy) && app.CreatedBy.Contains('@'))
            {
                var norm = app.CreatedBy.Trim().ToUpperInvariant();
                applicantUser = await userRepo.GetAll(u => !u.IsDeleted && u.NormalizedEmail == norm)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            // 3. Fallback: try ContactEmail if available
            if (applicantUser == null && !string.IsNullOrWhiteSpace(app.ContactEmail) && app.ContactEmail.Contains('@'))
            {
                var normContact = app.ContactEmail.Trim().ToUpperInvariant();
                applicantUser = await userRepo.GetAll(u => !u.IsDeleted && u.NormalizedEmail == normContact)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var applicantDto = applicantUser != null && !applicantUser.IsDeleted
                ? new ApplicantUserDto
                {
                    Id = applicantUser.Id,
                    FullName = applicantUser.FullName,
                    Email = applicantUser.Email ?? string.Empty,
                    PhoneNumber = applicantUser.PhoneNumber,
                    UserType = applicantUser.UserType,
                    IsActive = applicantUser.IsActive,
                    EmailConfirmed = applicantUser.EmailConfirmed,
                    CreatedAt = applicantUser.CreatedAt
                }
                : null;

            var dto = new DistributorApplicationDto
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
                ApplicantUser = applicantDto
            };

            return Result<DistributorApplicationDto>.Success(dto, LocalizationKeys.DistributorApplication.Fetched);
        }
    }
}

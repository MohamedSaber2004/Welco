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

            // Related applicant account: CreatedBy holds the signup email (MarkAsCreated),
            // ContactEmail may be the company email instead — prefer CreatedBy.
            var applicantEmail = !string.IsNullOrWhiteSpace(app.CreatedBy)
                ? app.CreatedBy.Trim()
                : app.ContactEmail.Trim();
            var normalizedApplicant = applicantEmail.ToUpperInvariant();
            var applicant = await _unitOfWork.GetRepository<ApplicationUser, Guid>()
                .GetBy(u => u.NormalizedEmail == normalizedApplicant)
                .Select(u => new ApplicantUserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber,
                    UserType = u.UserType,
                    IsActive = u.IsActive,
                    EmailConfirmed = u.EmailConfirmed,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

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
                ApplicantUser = applicant
            };

            return Result<DistributorApplicationDto>.Success(dto, LocalizationKeys.DistributorApplication.Fetched);
        }
    }
}

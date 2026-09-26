using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.DistributorApplications.Queries.GetDistributorApplications
{
    public class GetDistributorApplicationsQueryHandler : IRequestHandler<GetDistributorApplicationsQuery, PaginatedResult<DistributorApplicationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDistributorApplicationsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResult<DistributorApplicationDto>> Handle(GetDistributorApplicationsQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
            var query = repo.GetAll(a => !a.IsDeleted).Include(a => a.Country).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(a =>
                    a.CompanyName.ToLower().Contains(term) ||
                    a.ContactPerson.ToLower().Contains(term) ||
                    a.ContactEmail.ToLower().Contains(term));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(a => a.Status == request.Status.Value);
            }

            // Fetch raw apps with CreatedBy info so we can hydrate ApplicantUser
            var totalCount = await query.CountAsync(cancellationToken);
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var apps = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id, a.CompanyName, a.Type, a.CountryId,
                    CountryNameEn = a.Country != null ? a.Country.NameEn : null,
                    a.SalesVolumeBand, a.CategoryInterest, a.Website,
                    a.ContactPerson, a.ContactEmail, a.Phone, a.Status,
                    a.CreatedAt, a.UpdatedAt, a.CreatedBy
                })
                .ToListAsync(cancellationToken);

            // Resolve applicant users
            var userRepo = _unitOfWork.GetRepository<ApplicationUser, Guid>();
            var dtos = new List<DistributorApplicationDto>();

            foreach (var a in apps)
            {
                ApplicationUser? applicant = null;

                if (!string.IsNullOrWhiteSpace(a.CreatedBy))
                {
                    if (Guid.TryParse(a.CreatedBy.Trim(), out var cById) && cById != Guid.Empty)
                        applicant = await userRepo.GetAll(u => !u.IsDeleted && u.Id == cById).FirstOrDefaultAsync(cancellationToken);

                    if (applicant == null && a.CreatedBy.Contains('@'))
                    {
                        var norm = a.CreatedBy.Trim().ToUpperInvariant();
                        applicant = await userRepo.GetAll(u => !u.IsDeleted && u.NormalizedEmail == norm).FirstOrDefaultAsync(cancellationToken);
                    }
                }

                if (applicant == null && !string.IsNullOrWhiteSpace(a.ContactEmail) && a.ContactEmail.Contains('@'))
                {
                    var normContact = a.ContactEmail.Trim().ToUpperInvariant();
                    applicant = await userRepo.GetAll(u => !u.IsDeleted && u.NormalizedEmail == normContact).FirstOrDefaultAsync(cancellationToken);
                }

                dtos.Add(new DistributorApplicationDto
                {
                    Id = a.Id,
                    CompanyName = a.CompanyName,
                    Type = a.Type,
                    CountryId = a.CountryId,
                    CountryNameEn = a.CountryNameEn,
                    SalesVolumeBand = a.SalesVolumeBand,
                    CategoryInterest = a.CategoryInterest,
                    Website = a.Website,
                    ContactPerson = a.ContactPerson,
                    ContactEmail = a.ContactEmail,
                    Phone = a.Phone,
                    Status = a.Status.ToString(),
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    ApplicantUser = applicant != null && !applicant.IsDeleted ? new ApplicantUserDto
                    {
                        Id = applicant.Id,
                        FullName = applicant.FullName,
                        Email = applicant.Email ?? string.Empty,
                        PhoneNumber = applicant.PhoneNumber,
                        UserType = applicant.UserType,
                        IsActive = applicant.IsActive,
                        EmailConfirmed = applicant.EmailConfirmed,
                        CreatedAt = applicant.CreatedAt
                    } : null
                });
            }

            return PaginatedResult<DistributorApplicationDto>.Success(
                dtos, totalCount, pageNumber, pageSize,
                LocalizationKeys.DistributorApplication.ListFetched);
        }
    }
}

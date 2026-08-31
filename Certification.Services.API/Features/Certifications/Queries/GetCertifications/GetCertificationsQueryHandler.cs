using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Certifications;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CertificationEntity = Welco.Shared.Domain.Models.Certification;

namespace Certification.Services.API.Features.Certifications.Queries.GetCertifications
{
    public class GetCertificationsQueryHandler : IRequestHandler<GetCertificationsQuery, PaginatedResult<CertificationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCertificationsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResult<CertificationDto>> Handle(GetCertificationsQuery request, CancellationToken cancellationToken)
        {
            var certificationRepo = _unitOfWork.GetRepository<CertificationEntity, Guid>();
            var query = certificationRepo.GetAll(c => !c.IsDeleted).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(c =>
                    c.CertificateNumber.ToLower().Contains(term) ||
                    c.Title.ToLower().Contains(term) ||
                    c.IssuedTo.ToLower().Contains(term) ||
                    c.Issuer.ToLower().Contains(term));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == request.IsActive.Value);
            }

            return await query
                .OrderByDescending(c => c.IssueDate)
                .ToPaginatedListAsync(
                    c => new CertificationDto
                    {
                        Id = c.Id,
                        CertificateNumber = c.CertificateNumber,
                        Title = c.Title,
                        IssuedTo = c.IssuedTo,
                        Issuer = c.Issuer,
                        IssueDate = c.IssueDate,
                        ExpiryDate = c.ExpiryDate,
                        Description = c.Description,
                        CertificationImageName = c.CertificationImageName,
                        OwnerUserId = c.OwnerUserId,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    },
                    request.PageNumber,
                    request.PageSize,
                    LocalizationKeys.Certification.ListFetched,
                    cancellationToken);
        }
    }
}

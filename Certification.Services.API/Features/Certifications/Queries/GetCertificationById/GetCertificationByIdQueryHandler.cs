using MediatR;
using Welco.Shared.Common.DTOs.Certifications;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CertificationEntity = Welco.Shared.Domain.Models.Certification;

namespace Certification.Services.API.Features.Certifications.Queries.GetCertificationById
{
    public class GetCertificationByIdQueryHandler : IRequestHandler<GetCertificationByIdQuery, Result<CertificationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCertificationByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CertificationDto>> Handle(GetCertificationByIdQuery request, CancellationToken cancellationToken)
        {
            var certificationRepo = _unitOfWork.GetRepository<CertificationEntity, Guid>();
            var certification = await certificationRepo.GetByIdAsync(request.Id, cancellationToken);

            if (certification == null || certification.IsDeleted)
            {
                return Result<CertificationDto>.NotFound(LocalizationKeys.Certification.NotFound);
            }

            return Result<CertificationDto>.Success(ToDto(certification), LocalizationKeys.Certification.Fetched);
        }

        internal static CertificationDto ToDto(CertificationEntity certification)
        {
            return new CertificationDto
            {
                Id = certification.Id,
                CertificateNumber = certification.CertificateNumber,
                Title = certification.Title,
                IssuedTo = certification.IssuedTo,
                Issuer = certification.Issuer,
                IssueDate = certification.IssueDate,
                ExpiryDate = certification.ExpiryDate,
                Description = certification.Description,
                CertificationImageName = certification.CertificationImageName,
                OwnerUserId = certification.OwnerUserId,
                IsActive = certification.IsActive,
                CreatedAt = certification.CreatedAt,
                UpdatedAt = certification.UpdatedAt
            };
        }
    }
}

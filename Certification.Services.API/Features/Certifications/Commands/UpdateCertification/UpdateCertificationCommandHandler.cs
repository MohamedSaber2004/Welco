using MediatR;
using Welco.Shared.Common.DTOs.Certifications;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CertificationEntity = Welco.Shared.Domain.Models.Certification;

namespace Certification.Services.API.Features.Certifications.Commands.UpdateCertification
{
    public class UpdateCertificationCommandHandler : IRequestHandler<UpdateCertificationCommand, Result<CertificationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCertificationCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CertificationDto>> Handle(UpdateCertificationCommand request, CancellationToken cancellationToken)
        {
            var certificationRepo = _unitOfWork.GetRepository<CertificationEntity, Guid>();
            var certification = await certificationRepo.GetByIdAsync(request.Id, cancellationToken);

            if (certification == null || certification.IsDeleted)
            {
                return Result<CertificationDto>.NotFound(LocalizationKeys.Certification.NotFound);
            }

            var certificateNumber = request.CertificateNumber.Trim();
            if (!string.Equals(certification.CertificateNumber, certificateNumber, StringComparison.OrdinalIgnoreCase))
            {
                var numberExists = await certificationRepo.ExistsAsync(
                    c => !c.IsDeleted && c.Id != request.Id && c.CertificateNumber.ToLower() == certificateNumber.ToLower(),
                    cancellationToken);

                if (numberExists)
                {
                    return Result<CertificationDto>.Conflict(LocalizationKeys.Certification.CertificateNumberAlreadyExists);
                }
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            certification.Update(
                certificateNumber,
                request.Title.Trim(),
                request.IssuedTo.Trim(),
                request.Issuer.Trim(),
                request.IssueDate,
                request.ExpiryDate,
                request.Description,
                request.CertificationImageName,
                currentUserId);

            if (request.IsActive.HasValue)
            {
                certification.SetActiveState(request.IsActive.Value, currentUserId);
            }

            certificationRepo.Update(certification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CertificationDto>.Success(ToDto(certification), LocalizationKeys.Certification.Updated);
        }

        private static CertificationDto ToDto(CertificationEntity certification)
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

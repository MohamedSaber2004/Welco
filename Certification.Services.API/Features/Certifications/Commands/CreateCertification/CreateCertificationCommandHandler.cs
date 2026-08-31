using MediatR;
using Welco.Shared.Common.DTOs.Certifications;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CertificationEntity = Welco.Shared.Domain.Models.Certification;

namespace Certification.Services.API.Features.Certifications.Commands.CreateCertification
{
    public class CreateCertificationCommandHandler : IRequestHandler<CreateCertificationCommand, Result<CertificationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateCertificationCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CertificationDto>> Handle(CreateCertificationCommand request, CancellationToken cancellationToken)
        {
            var certificateNumber = request.CertificateNumber.Trim();

            var certificationRepo = _unitOfWork.GetRepository<CertificationEntity, Guid>();

            var numberExists = await certificationRepo.ExistsAsync(
                c => !c.IsDeleted && c.CertificateNumber.ToLower() == certificateNumber.ToLower(),
                cancellationToken);

            if (numberExists)
            {
                return Result<CertificationDto>.Conflict(LocalizationKeys.Certification.CertificateNumberAlreadyExists);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            var certification = CertificationEntity.Create(
                certificateNumber,
                request.Title.Trim(),
                request.IssuedTo.Trim(),
                request.Issuer.Trim(),
                request.IssueDate,
                request.ExpiryDate,
                request.Description,
                request.CertificationImageName,
                _currentUserService.UserId != Guid.Empty ? _currentUserService.UserId : null,
                currentUserId);

            await certificationRepo.AddAsync(certification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CertificationDto>.Created(ToDto(certification), LocalizationKeys.Certification.Created);
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

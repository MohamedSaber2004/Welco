using MediatR;
using Welco.Shared.Common.DTOs.Providers;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProviderEntity = Welco.Shared.Domain.Models.Provider;

namespace Provider.Services.API.Features.Providers.Commands.UpdateProvider
{
    public class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, Result<ProviderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProviderCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ProviderDto>> Handle(UpdateProviderCommand request, CancellationToken cancellationToken)
        {
            var providerRepo = _unitOfWork.GetRepository<ProviderEntity, Guid>();
            var provider = await providerRepo.GetByIdAsync(request.Id, cancellationToken);
            if (provider == null || provider.IsDeleted)
            {
                return Result<ProviderDto>.NotFound(LocalizationKeys.Provider.NotFound);
            }

            if (!string.IsNullOrWhiteSpace(request.CommercialRegistrationNumber)
                && !string.Equals(provider.CommercialRegistrationNumber, request.CommercialRegistrationNumber.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var crnExists = await providerRepo.ExistsAsync(
                    p => !p.IsDeleted && p.Id != request.Id && p.CommercialRegistrationNumber != null
                        && p.CommercialRegistrationNumber.ToLower() == request.CommercialRegistrationNumber.Trim().ToLower(),
                    cancellationToken);

                if (crnExists)
                {
                    return Result<ProviderDto>.Conflict(LocalizationKeys.Provider.CommercialRegistrationNumberAlreadyExists);
                }
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            provider.Update(
                request.CommercialName,
                request.CompanyName,
                request.CommercialRegistrationNumber,
                request.ContactPersonName,
                request.ContactPersonPhone,
                request.Phone,
                request.Email,
                request.Address,
                request.Notes,
                request.ImageName,
                null,
                currentUserId);

            if (request.IsActive.HasValue)
            {
                provider.SetActiveState(request.IsActive.Value, currentUserId);
            }

            providerRepo.Update(provider);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new ProviderDto
            {
                Id = provider.Id,
                CommercialName = provider.CommercialName,
                CompanyName = provider.CompanyName,
                CommercialRegistrationNumber = provider.CommercialRegistrationNumber,
                ContactPersonName = provider.ContactPersonName,
                ContactPersonPhone = provider.ContactPersonPhone,
                Phone = provider.Phone,
                Email = provider.Email,
                Address = provider.Address,
                Notes = provider.Notes,
                ImageName = provider.ImageName,
                OwnerUserId = provider.OwnerUserId,
                IsActive = provider.IsActive,
                CreatedAt = provider.CreatedAt,
                UpdatedAt = provider.UpdatedAt
            };

            return Result<ProviderDto>.Success(dto, LocalizationKeys.Provider.Updated);
        }
    }
}

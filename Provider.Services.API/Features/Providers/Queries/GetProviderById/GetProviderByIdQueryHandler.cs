using MediatR;
using Welco.Shared.Common.DTOs.Providers;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProviderEntity = Welco.Shared.Domain.Models.Provider;

namespace Provider.Services.API.Features.Providers.Queries.GetProviderById
{
    public class GetProviderByIdQueryHandler : IRequestHandler<GetProviderByIdQuery, Result<ProviderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProviderByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ProviderDto>> Handle(GetProviderByIdQuery request, CancellationToken cancellationToken)
        {
            var providerRepo = _unitOfWork.GetRepository<ProviderEntity, Guid>();
            var provider = await providerRepo.GetByIdAsync(request.Id, cancellationToken);
            if (provider == null || provider.IsDeleted)
            {
                return Result<ProviderDto>.NotFound(LocalizationKeys.Provider.NotFound);
            }

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

            return Result<ProviderDto>.Success(dto, LocalizationKeys.Provider.Fetched);
        }
    }
}

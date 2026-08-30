using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Providers;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProviderEntity = Welco.Shared.Domain.Models.Provider;

namespace Provider.Services.API.Features.Providers.Queries.GetProviders
{
    public class GetProvidersQueryHandler : IRequestHandler<GetProvidersQuery, PaginatedResult<ProviderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProvidersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResult<ProviderDto>> Handle(GetProvidersQuery request, CancellationToken cancellationToken)
        {
            var providerRepo = _unitOfWork.GetRepository<ProviderEntity, Guid>();
            var query = providerRepo.GetAll(p => !p.IsDeleted).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.CommercialName.ToLower().Contains(term) ||
                    (p.CompanyName != null && p.CompanyName.ToLower().Contains(term)) ||
                    (p.ContactPersonName != null && p.ContactPersonName.ToLower().Contains(term)) ||
                    (p.Email != null && p.Email.ToLower().Contains(term)) ||
                    (p.CommercialRegistrationNumber != null && p.CommercialRegistrationNumber.ToLower().Contains(term)));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == request.IsActive.Value);
            }

            return await query
                .OrderBy(p => p.CommercialName)
                .ToPaginatedListAsync(
                    p => new ProviderDto
                    {
                        Id = p.Id,
                        CommercialName = p.CommercialName,
                        CompanyName = p.CompanyName,
                        CommercialRegistrationNumber = p.CommercialRegistrationNumber,
                        ContactPersonName = p.ContactPersonName,
                        ContactPersonPhone = p.ContactPersonPhone,
                        Phone = p.Phone,
                        Email = p.Email,
                        Address = p.Address,
                        Notes = p.Notes,
                        ImageName = p.ImageName,
                        OwnerUserId = p.OwnerUserId,
                        IsActive = p.IsActive,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    },
                    request.PageNumber,
                    request.PageSize,
                    LocalizationKeys.Provider.ListFetched,
                    cancellationToken);
        }
    }
}

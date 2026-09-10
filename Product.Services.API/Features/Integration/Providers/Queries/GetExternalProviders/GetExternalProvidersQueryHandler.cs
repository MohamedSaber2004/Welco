using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Results;
using CompanyEntity = Welco.Shared.Domain.Models.Company;

namespace Product.Services.API.Features.Integration.Providers.Queries.GetExternalProviders
{
    public class GetExternalProvidersQueryHandler : IRequestHandler<GetExternalProvidersQuery, Result<List<ExternalProviderDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetExternalProvidersQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<List<ExternalProviderDto>>> Handle(GetExternalProvidersQuery request, CancellationToken ct)
        {
            var companyRepo = _uow.GetRepository<CompanyEntity, Guid>();
            var providers = await companyRepo
                .GetAll(c => !c.IsDeleted && c.IsProvider)
                .Select(c => new ExternalProviderDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Status = c.Status.ToString(),
                    IsProvider = c.IsProvider
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return Result<List<ExternalProviderDto>>.Success(providers);
        }
    }
}

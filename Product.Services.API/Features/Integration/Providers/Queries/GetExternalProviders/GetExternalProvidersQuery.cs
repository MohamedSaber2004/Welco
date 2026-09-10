using MediatR;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Integration.Providers.Queries.GetExternalProviders
{
    public class GetExternalProvidersQuery : IRequest<Result<List<ExternalProviderDto>>>
    {
    }
}

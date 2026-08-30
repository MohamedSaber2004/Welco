using MediatR;
using Welco.Shared.Common.DTOs.Providers;
using Welco.Shared.Results;

namespace Provider.Services.API.Features.Providers.Queries.GetProviders
{
    public class GetProvidersQuery : IRequest<PaginatedResult<ProviderDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
    }
}

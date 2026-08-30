using MediatR;
using Welco.Shared.Common.DTOs.Providers;
using Welco.Shared.Results;

namespace Provider.Services.API.Features.Providers.Queries.GetProviderById
{
    public class GetProviderByIdQuery : IRequest<Result<ProviderDto>>
    {
        public Guid Id { get; set; }
    }
}

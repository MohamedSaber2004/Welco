using MediatR;
using Welco.Shared.Results;

namespace Provider.Services.API.Features.Providers.Commands.DeleteProvider
{
    public class DeleteProviderCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}

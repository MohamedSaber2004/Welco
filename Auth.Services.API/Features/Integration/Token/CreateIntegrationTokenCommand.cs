using MediatR;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Integration.Token
{
    public class CreateIntegrationTokenCommand : IRequest<Result<IntegrationTokenResponse>>
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}

using MediatR;
using Welco.Shared.Common.DTOs.Providers;
using Welco.Shared.Results;

namespace Provider.Services.API.Features.Providers.Commands.CreateProvider
{
    public class CreateProviderCommand : IRequest<Result<ProviderDto>>
    {
        public string CommercialName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? CommercialRegistrationNumber { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonPhone { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Notes { get; set; }
        public string? ImageName { get; set; }
    }
}

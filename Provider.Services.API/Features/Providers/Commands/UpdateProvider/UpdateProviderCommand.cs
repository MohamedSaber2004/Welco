using MediatR;
using Welco.Shared.Common.DTOs.Providers;
using Welco.Shared.Results;

namespace Provider.Services.API.Features.Providers.Commands.UpdateProvider
{
    public class UpdateProviderCommand : IRequest<Result<ProviderDto>>
    {
        public Guid Id { get; set; }
        public string? CommercialName { get; set; }
        public string? CompanyName { get; set; }
        public string? CommercialRegistrationNumber { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonPhone { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
        public string? ImageName { get; set; }
        public bool? IsActive { get; set; }
    }
}

using MediatR;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Integration.Orders.Commands.UpdateExternalOrderStatus
{
    public class UpdateExternalOrderStatusCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
    }
}

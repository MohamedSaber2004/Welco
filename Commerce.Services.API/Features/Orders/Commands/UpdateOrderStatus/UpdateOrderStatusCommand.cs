using MediatR;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Orders.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

using MediatR;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Integration.Inventory.Commands.ReserveInventory
{
    public class ReserveInventoryCommand : IRequest<Result<InventoryCheckResponse>>
    {
        public List<InventoryCheckItem> Items { get; set; } = new();
    }
}

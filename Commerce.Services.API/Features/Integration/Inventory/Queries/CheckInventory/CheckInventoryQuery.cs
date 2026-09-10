using MediatR;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Integration.Inventory.Queries.CheckInventory
{
    public class CheckInventoryQuery : IRequest<Result<InventoryCheckResponse>>
    {
        public List<InventoryCheckItem> Items { get; set; } = new();
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Commerce.Services.API.Features.Integration.Inventory.Commands.ReserveInventory
{
    public class ReserveInventoryCommandHandler : IRequestHandler<ReserveInventoryCommand, Result<InventoryCheckResponse>>
    {
        private readonly IUnitOfWork _uow;

        public ReserveInventoryCommandHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<InventoryCheckResponse>> Handle(ReserveInventoryCommand request, CancellationToken ct)
        {
            var productRepo = _uow.GetRepository<ProductEntity, Guid>();
            var results = new List<InventoryItemResult>();

            foreach (var item in request.Items ?? new List<InventoryCheckItem>())
            {
                var product = await productRepo.GetAll(p => !p.IsDeleted && p.Id == item.WelcoProductId)
                    .FirstOrDefaultAsync(ct);

                if (product == null)
                {
                    results.Add(new InventoryItemResult { WelcoProductId = item.WelcoProductId, AvailableStock = 0, IsAvailable = false });
                    continue;
                }

                if (product.Stock < item.RequestedQuantity)
                {
                    results.Add(new InventoryItemResult { WelcoProductId = item.WelcoProductId, AvailableStock = product.Stock, IsAvailable = false });
                    continue;
                }

                product.Stock -= item.RequestedQuantity;
                product.MarkAsUpdated("integration-service");
                results.Add(new InventoryItemResult { WelcoProductId = item.WelcoProductId, AvailableStock = product.Stock, IsAvailable = true });
            }

            if (results.All(r => r.IsAvailable))
            {
                await _uow.SaveChangesAsync(ct);
            }

            return Result<InventoryCheckResponse>.Success(new InventoryCheckResponse
            {
                AllAvailable = results.All(r => r.IsAvailable),
                Items = results
            });
        }
    }
}

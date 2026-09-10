using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Commerce.Services.API.Features.Integration.Inventory.Queries.CheckInventory
{
    public class CheckInventoryQueryHandler : IRequestHandler<CheckInventoryQuery, Result<InventoryCheckResponse>>
    {
        private readonly IUnitOfWork _uow;

        public CheckInventoryQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<InventoryCheckResponse>> Handle(CheckInventoryQuery request, CancellationToken ct)
        {
            var productRepo = _uow.GetRepository<ProductEntity, Guid>();
            var results = new List<InventoryItemResult>();

            foreach (var item in request.Items ?? new List<InventoryCheckItem>())
            {
                var product = await productRepo.GetAll(p => !p.IsDeleted && p.Id == item.WelcoProductId)
                    .Select(p => new { p.Id, p.Stock })
                    .FirstOrDefaultAsync(ct);

                results.Add(new InventoryItemResult
                {
                    WelcoProductId = item.WelcoProductId,
                    AvailableStock = product?.Stock ?? 0,
                    IsAvailable = product != null && product.Stock >= item.RequestedQuantity
                });
            }

            return Result<InventoryCheckResponse>.Success(new InventoryCheckResponse
            {
                AllAvailable = results.All(r => r.IsAvailable),
                Items = results
            });
        }
    }
}

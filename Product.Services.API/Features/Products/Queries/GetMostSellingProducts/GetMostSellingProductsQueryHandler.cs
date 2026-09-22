using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Services.API.Common;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Products.Queries.GetMostSellingProducts
{
    public class GetMostSellingProductsQueryHandler : IRequestHandler<GetMostSellingProductsQuery, Result<IReadOnlyList<ProductDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMostSellingProductsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<ProductDto>>> Handle(GetMostSellingProductsQuery request, CancellationToken cancellationToken)
        {
            var limit = request.Limit <= 0 ? 7 : Math.Min(request.Limit, 50);

            var orderItemRepo = _unitOfWork.GetRepository<OrderItem, Guid>();
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();

            // Aggregate order items to find products with the highest sold quantities
            var topProductQuantities = await orderItemRepo.GetAll(oi => !oi.IsDeleted && (oi.Order == null || (oi.Order.Status != OrderStatus.Cancelled && oi.Order.Status != OrderStatus.Rejected)))
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(limit)
                .ToListAsync(cancellationToken);

            var topProductIds = topProductQuantities.Select(x => x.ProductId).ToList();

            var productsQuery = productRepo.GetAll(p => !p.IsDeleted && p.IsActive);

            var resultProducts = new List<ProductDto>();

            if (topProductIds.Count > 0)
            {
                var fetchedTopProducts = await productsQuery
                    .Where(p => topProductIds.Contains(p.Id))
                    .Select(ProductDtoMapper.Projection)
                    .ToListAsync(cancellationToken);

                // Preserve rank ordering according to sales quantity
                resultProducts = topProductIds
                    .Select(id => fetchedTopProducts.FirstOrDefault(p => p.Id == id))
                    .Where(p => p != null)
                    .Select(p => p!)
                    .ToList();
            }

            // If fewer than limit, backfill with active catalog products (in-stock first, then newest)
            if (resultProducts.Count < limit)
            {
                var existingIds = resultProducts.Select(p => p.Id).ToHashSet();
                var remainingCount = limit - resultProducts.Count;

                var backfillProducts = await productsQuery
                    .Where(p => !existingIds.Contains(p.Id))
                    .OrderByDescending(p => p.Stock > 0)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(remainingCount)
                    .Select(ProductDtoMapper.Projection)
                    .ToListAsync(cancellationToken);

                resultProducts.AddRange(backfillProducts);
            }

            return Result<IReadOnlyList<ProductDto>>.Success(resultProducts, LocalizationKeys.Product.ListFetched);
        }
    }
}

using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Results;
using OrderEntity = Welco.Shared.Domain.Models.Order;
using OrderItemEntity = Welco.Shared.Domain.Models.OrderItem;
using ProductEntity = Welco.Shared.Domain.Models.Product;
using CurrencyEntity = Welco.Shared.Domain.Models.Currency;

namespace Commerce.Services.API.Features.Integration.Orders.Commands.CreateExternalOrder
{
    public class CreateExternalOrderCommandHandler : IRequestHandler<CreateExternalOrderCommand, Result<ExternalOrderResponse>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CreateExternalOrderCommandHandler> _logger;

        public CreateExternalOrderCommandHandler(
            IUnitOfWork uow,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CreateExternalOrderCommandHandler> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Result<ExternalOrderResponse>> Handle(CreateExternalOrderCommand request, CancellationToken ct)
        {
            _logger.LogInformation("[Integration MediatR] CreateExternalOrder Market={Market} CurrencyId={CurrencyId}",
                request.SourceMarket, request.CurrencyId);

            if (request.Items == null || !request.Items.Any())
            {
                return Result<ExternalOrderResponse>.BadRequest("Order must contain at least one item.");
            }

if (request.CurrencyId.HasValue)
            {
                var currencyRepo = _uow.GetRepository<CurrencyEntity, Guid>();
                var currencyExists = await currencyRepo.ExistsAsync(c => !c.IsDeleted && c.Id == request.CurrencyId.Value, ct);
                if (!currencyExists)
                {
                    _logger.LogWarning("[Integration MediatR] Currency {CurrencyId} not found.", request.CurrencyId.Value);
                    return Result<ExternalOrderResponse>.NotFound($"Currency {request.CurrencyId.Value} not found.");
                }
            }

var productRepo = _uow.GetRepository<ProductEntity, Guid>();
            foreach (var item in request.Items)
            {
                var exists = await productRepo.ExistsAsync(p => !p.IsDeleted && p.Id == item.WelcoProductId, ct);
                if (!exists)
                {
                    _logger.LogWarning("[Integration MediatR] Product {ProductId} not found.", item.WelcoProductId);
                    return Result<ExternalOrderResponse>.NotFound($"Product {item.WelcoProductId} not found.");
                }
            }

            var order = new OrderEntity
            {
                OrderNumber = $"PK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                Status = OrderStatus.Confirmed,
                UserId = null,
                CompanyId = null,
                CurrencyId = request.CurrencyId,
                TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice),
                SourceMarket = request.SourceMarket ?? "Egypt"
            };
            order.MarkAsCreated("integration-service");

            foreach (var item in request.Items)
            {
                var orderItem = new OrderItemEntity
                {
                    OrderId = order.Id,
                    ProductId = item.WelcoProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                orderItem.MarkAsCreated("integration-service");
                order.Items.Add(orderItem);
            }

            var orderRepo = _uow.GetRepository<OrderEntity, Guid>();
            await orderRepo.AddAsync(order, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("[Integration MediatR] Order created WelcoOrderId={WelcoOrderId} WelcoOrderNumber={WelcoOrderNumber}",
                order.Id, order.OrderNumber);

            return Result<ExternalOrderResponse>.Created(new ExternalOrderResponse
            {
                WelcoOrderId = order.Id,
                WelcoOrderNumber = order.OrderNumber,
                Status = order.Status.ToString()
            });
        }
    }
}

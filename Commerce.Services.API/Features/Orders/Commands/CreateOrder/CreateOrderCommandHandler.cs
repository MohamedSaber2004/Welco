using Commerce.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using OrderEntity = Welco.Shared.Domain.Models.Order;
using OrderItemEntity = Welco.Shared.Domain.Models.OrderItem;
using ProductEntity = Welco.Shared.Domain.Models.Product;
using CompanyEntity = Welco.Shared.Domain.Models.Company;
using CurrencyEntity = Welco.Shared.Domain.Models.Currency;

namespace Commerce.Services.API.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public CreateOrderCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            if (request.Items == null || !request.Items.Any())
                return Result<OrderDto>.BadRequest(LocalizationKeys.Order.ItemsRequired);

            if (request.CompanyId.HasValue)
            {
                var companyRepo = _uow.GetRepository<CompanyEntity, Guid>();
                var exists = await companyRepo.ExistsAsync(c => !c.IsDeleted && c.Id == request.CompanyId.Value, cancellationToken);
                if (!exists) return Result<OrderDto>.NotFound(LocalizationKeys.Company.NotFound);
            }

            if (request.CurrencyId.HasValue)
            {
                var currencyRepo = _uow.GetRepository<CurrencyEntity, Guid>();
                var exists = await currencyRepo.ExistsAsync(c => !c.IsDeleted && c.Id == request.CurrencyId.Value, cancellationToken);
                if (!exists) return Result<OrderDto>.BadRequest(LocalizationKeys.Currency.NotFound);
            }

            var productRepo = _uow.GetRepository<ProductEntity, Guid>();
            foreach (var it in request.Items)
            {
                var exists = await productRepo.ExistsAsync(p => !p.IsDeleted && p.Id == it.ProductId, cancellationToken);
                if (!exists) return Result<OrderDto>.NotFound(LocalizationKeys.Product.NotFound);
            }

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";

            var order = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                Status = Welco.Shared.Domain.Models.OrderStatus.Pending,
                UserId = request.UserId,
                CompanyId = request.CompanyId,
                CurrencyId = request.CurrencyId,
                QuoteId = request.QuoteId,
                TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice)
            };
            order.MarkAsCreated(currentUserId);

            foreach (var it in request.Items)
            {
                var orderItem = new OrderItemEntity
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = it.ProductId,
                    Quantity = it.Quantity,
                    UnitPrice = it.UnitPrice
                };
                orderItem.MarkAsCreated(currentUserId);
                order.Items.Add(orderItem);
            }

            var repo = _uow.GetRepository<OrderEntity, Guid>();
            await repo.AddAsync(order, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var saved = await repo.GetAll(o => !o.IsDeleted && o.Id == order.Id)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(cancellationToken);

            var dto = CommerceDtoMapper.ToDto(saved!);
            return Result<OrderDto>.Created(dto, LocalizationKeys.Order.Created);
        }
    }
}

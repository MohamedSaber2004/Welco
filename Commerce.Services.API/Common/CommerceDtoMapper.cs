using System.Linq.Expressions;
using Welco.Shared.Common.DTOs.Commerce;
using CartEntity = Welco.Shared.Domain.Models.Cart;
using CartItemEntity = Welco.Shared.Domain.Models.CartItem;
using OrderEntity = Welco.Shared.Domain.Models.Order;
using OrderItemEntity = Welco.Shared.Domain.Models.OrderItem;

namespace Commerce.Services.API.Common
{
    internal static class CommerceDtoMapper
    {
        public static Expression<Func<CartEntity, CartDto>> CartProjection => c => new CartDto
        {
            Id = c.Id,
            UserId = c.UserId,
            SessionId = c.SessionId,
            CurrencyId = c.CurrencyId,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            Items = c.Items.Where(i => !i.IsDeleted).Select(i => new CartItemDto
            {
                Id = i.Id,
                CartId = i.CartId,
                ProductId = i.ProductId,
                ProductNameEn = i.Product != null ? i.Product.NameEn : null,
                Quantity = i.Quantity,
                UnitPriceSnapshot = i.UnitPriceSnapshot
            }).ToList()
        };

        public static CartDto ToDto(CartEntity c)
        {
            return new CartDto
            {
                Id = c.Id,
                UserId = c.UserId,
                SessionId = c.SessionId,
                CurrencyId = c.CurrencyId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                Items = c.Items.Where(i => !i.IsDeleted).Select(i => new CartItemDto
                {
                    Id = i.Id,
                    CartId = i.CartId,
                    ProductId = i.ProductId,
                    ProductNameEn = i.Product != null ? i.Product.NameEn : null,
                    Quantity = i.Quantity,
                    UnitPriceSnapshot = i.UnitPriceSnapshot
                }).ToList()
            };
        }

        public static Expression<Func<OrderEntity, OrderDto>> OrderProjection => o => new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Status = o.Status.ToString(),
            UserId = o.UserId,
            CompanyId = o.CompanyId,
            CurrencyId = o.CurrencyId,
            TotalAmount = o.TotalAmount,
            IsActive = o.IsActive,
            CreatedAt = o.CreatedAt,
            Items = o.Items.Where(i => !i.IsDeleted).Select(i => new OrderItemDto
            {
                Id = i.Id,
                OrderId = i.OrderId,
                ProductId = i.ProductId,
                ProductNameEn = i.Product != null ? i.Product.NameEn : null,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        public static OrderDto ToDto(OrderEntity o)
        {
            return new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status.ToString(),
                UserId = o.UserId,
                CompanyId = o.CompanyId,
                CurrencyId = o.CurrencyId,
                TotalAmount = o.TotalAmount,
                IsActive = o.IsActive,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Where(i => !i.IsDeleted).Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    OrderId = i.OrderId,
                    ProductId = i.ProductId,
                    ProductNameEn = i.Product != null ? i.Product.NameEn : null,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }
    }
}

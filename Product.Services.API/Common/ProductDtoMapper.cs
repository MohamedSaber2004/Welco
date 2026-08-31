using System.Linq.Expressions;
using Welco.Shared.Common.DTOs.Products;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Common
{
    internal static class ProductDtoMapper
    {
        public static Expression<Func<ProductEntity, ProductDto>> Projection => p => new ProductDto
        {
            Id = p.Id,
            NameEn = p.NameEn,
            NameAr = p.NameAr,
            Sku = p.Sku,
            Slug = p.Slug,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            Specifications = p.Specifications,
            ImageName = p.ImageName,
            Material = p.Material,
            LengthCm = p.LengthCm,
            CurrencyId = p.CurrencyId,
            CurrencyCode = p.Currency != null ? p.Currency.Code : null,
            CurrencySymbol = p.Currency != null ? p.Currency.Symbol : null,
            CategoryId = p.CategoryId,
            CategoryNameEn = p.Category != null ? p.Category.NameEn : null,
            CategoryNameAr = p.Category != null ? p.Category.NameAr : null,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt

        };
    }
}

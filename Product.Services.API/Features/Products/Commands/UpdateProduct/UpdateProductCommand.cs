using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommand : IRequest<Result<ProductDto>>
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? Specifications { get; set; }
        public string? ImageName { get; set; }
        public string? Material { get; set; }
        public decimal? LengthCm { get; set; }
        public Guid? CurrencyId { get; set; }
        public Guid CategoryId { get; set; }
        public bool? IsActive { get; set; }
    }
}

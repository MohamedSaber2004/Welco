using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class Product : BaseEntity<Guid>
    {
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? Specifications { get; set; }
        public string? ImageName { get; set; }
        public string? Material { get; set; }
        public decimal? LengthCm { get; set; }
        public Guid? CurrencyId { get; set; }
        public Guid CategoryId { get; set; }

        public virtual Currency? Currency { get; set; }
        public virtual Category? Category { get; set; }

        public static Product Create(
            string nameEn,
            string nameAr,
            string sku,
            string slug,
            string? description,
            decimal price,
            int stock,
            string? specifications,
            string? imageName,
            string? material,
            decimal? lengthCm,
            Guid? currencyId,
            Guid categoryId,
            string createdBy)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                NameEn = nameEn,
                NameAr = nameAr,
                Sku = sku,
                Slug = slug,
                Description = description,
                Price = price,
                Stock = stock,
                Specifications = specifications,
                ImageName = imageName,
                Material = material,
                LengthCm = lengthCm,
                CurrencyId = currencyId,
                CategoryId = categoryId
            };
            product.MarkAsCreated(createdBy);
            return product;
        }

        public void Update(
            string nameEn,
            string nameAr,
            string sku,
            string slug,
            string? description,
            decimal price,
            int stock,
            string? specifications,
            string? imageName,
            string? material,
            decimal? lengthCm,
            Guid? currencyId,
            Guid categoryId,
            string updatedBy)
        {
            NameEn = nameEn.Trim();
            NameAr = nameAr.Trim();
            Sku = sku.Trim();
            Slug = slug.Trim();
            if (description != null) Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Price = price;
            Stock = stock;
            if (specifications != null) Specifications = string.IsNullOrWhiteSpace(specifications) ? null : specifications.Trim();
            if (imageName != null) ImageName = string.IsNullOrWhiteSpace(imageName) ? null : imageName.Trim();
            if (material != null) Material = string.IsNullOrWhiteSpace(material) ? null : material.Trim();
            LengthCm = lengthCm;
            CurrencyId = currencyId;
            CategoryId = categoryId;
            MarkAsUpdated(updatedBy);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.NameEn)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.NameAr)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Sku)
                .IsRequired()
                .HasMaxLength(50);
            builder.HasIndex(x => x.Sku).IsUnique();

            builder.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(200);
            builder.HasIndex(x => x.Slug).IsUnique();

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.Property(x => x.Price)
                .HasPrecision(18, 2);

            builder.Property(x => x.LengthCm)
                .HasPrecision(10, 2);

            builder.Property(x => x.Material)
                .HasMaxLength(100);

            builder.Property(x => x.Specifications)
                .HasMaxLength(2000);

            builder.Property(x => x.ImageName)
                .HasMaxLength(500);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Currency)
                .WithMany()
                .HasForeignKey(x => x.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CategoryId);
            builder.HasIndex(x => x.CurrencyId);

            builder.Property(x => x.CreatedBy)
                .IsRequired();
        }
    }
}

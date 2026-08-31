using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecification>
    {
        public void Configure(EntityTypeBuilder<ProductSpecification> builder)
        {
            builder.ToTable("ProductSpecifications");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AttrName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.AttrValue).IsRequired().HasMaxLength(500);
            builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.ProductId);
            builder.Property(x => x.CreatedBy).IsRequired();
        }
    }
}

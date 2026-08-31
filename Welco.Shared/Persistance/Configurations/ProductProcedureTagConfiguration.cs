using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class ProductProcedureTagConfiguration : IEntityTypeConfiguration<ProductProcedureTag>
    {
        public void Configure(EntityTypeBuilder<ProductProcedureTag> builder)
        {
            builder.ToTable("ProductProcedureTags");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Label).IsRequired().HasMaxLength(200);
            builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.ProductId);
            builder.Property(x => x.CreatedBy).IsRequired();
        }
    }
}

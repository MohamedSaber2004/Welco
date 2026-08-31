using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class ProductMediaConfiguration : IEntityTypeConfiguration<ProductMedia>
    {
        public void Configure(EntityTypeBuilder<ProductMedia> builder)
        {
            builder.ToTable("ProductMedias");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).HasConversion<string>().IsRequired();
            builder.Property(x => x.Url).IsRequired().HasMaxLength(1000);
            builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.ProductId);
            builder.Property(x => x.CreatedBy).IsRequired();
        }
    }
}

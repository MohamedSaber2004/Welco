using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class UserProductInteractionConfiguration : IEntityTypeConfiguration<UserProductInteraction>
    {
        public void Configure(EntityTypeBuilder<UserProductInteraction> builder)
        {
            builder.ToTable("UserProductInteractions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).IsRequired().HasMaxLength(50);
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => new { x.UserId, x.ProductId, x.Type }).IsUnique();
            builder.Property(x => x.CreatedBy).IsRequired();
        }
    }
}

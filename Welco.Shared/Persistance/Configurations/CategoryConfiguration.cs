using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.NameEn)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.NameAr)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.ImageName)
                .HasMaxLength(500);

            builder.HasOne(x => x.ParentCategory)
                .WithMany(x => x.SubCategories)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ParentCategoryId, x.NameEn })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(x => new { x.ParentCategoryId, x.NameAr })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(x => x.CreatedBy)
                .IsRequired();
        }
    }
}
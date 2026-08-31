using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Type).HasConversion<string>().IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().IsRequired();
            builder.Property(x => x.TierLevel).IsRequired();
            builder.HasIndex(x => x.Name);
            builder.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AccountManager).WithMany().HasForeignKey(x => x.AccountManagerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Users).WithOne(u => u.Company).HasForeignKey(u => u.CompanyId).OnDelete(DeleteBehavior.SetNull);
            builder.Property(x => x.CreatedBy).IsRequired();
        }
    }
}

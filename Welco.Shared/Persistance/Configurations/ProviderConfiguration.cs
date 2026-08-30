using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
    {
        public void Configure(EntityTypeBuilder<Provider> builder)
        {
            builder.ToTable("Providers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CommercialName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.CompanyName)
                .HasMaxLength(200);

            builder.Property(x => x.CommercialRegistrationNumber)
                .HasMaxLength(50);

            builder.HasIndex(x => x.CommercialRegistrationNumber);

            builder.Property(x => x.ContactPersonName)
                .HasMaxLength(150);

            builder.Property(x => x.ContactPersonPhone)
                .HasMaxLength(30);

            builder.Property(x => x.Phone)
                .HasMaxLength(30);

            builder.Property(x => x.Email)
                .HasMaxLength(150);

            builder.Property(x => x.Address)
                .HasMaxLength(500);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.Property(x => x.ImageName)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedBy)
                .IsRequired();
        }
    }
}
